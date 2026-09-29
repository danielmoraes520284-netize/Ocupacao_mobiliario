using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace BimBlaster.OcupacaoMobiliario;

public class UserFacingException : Exception
{
    public UserFacingException(string message) : base(message) { }
}

public class OcupacaoOptions
{
    /// <summary>Null = mobiliário do próprio modelo; caso contrário, o vínculo escolhido.</summary>
    public RevitLinkInstance? Link { get; set; }
    public string FamilyFilter { get; set; } = "CADEIRA";
    public Phase Phase { get; set; } = null!;
    public string RoomParameterName { get; set; } = "";
    public string TotalParameterName { get; set; } = "";
}

public class OcupacaoResult
{
    public int Total { get; set; }
    public int RoomsWithChairs { get; set; }
    public int ChairsFound { get; set; }
    public int ChairsOutside { get; set; }
    public string SourceName { get; set; } = "";
    public string PhaseName { get; set; } = "";
    public string TotalStatus { get; set; } = "";
    public List<string> Warnings { get; } = new();

    public string ToMessage(AppLanguage lang)
    {
        var msg = LocalizedStrings.Format(lang, "ResultBody",
            Total, RoomsWithChairs, ChairsFound, ChairsOutside, SourceName, PhaseName, TotalStatus);
        if (Warnings.Count > 0)
            msg += LocalizedStrings.Format(lang, "ResultErrors", Warnings.Count, string.Join("\n", Warnings.Take(15)));
        return msg;
    }
}

public static class OcupacaoService
{
    /// <summary>Parâmetros graváveis (inteiro/número) de um elemento, sem repetir nomes.</summary>
    private static List<string> WritableParameterNames(Element element) =>
        element.Parameters.Cast<Parameter>()
            .Where(p => !p.IsReadOnly && p.StorageType is StorageType.Integer or StorageType.Double)
            .Select(p => p.Definition.Name)
            .Distinct().OrderBy(n => n, StringComparer.CurrentCultureIgnoreCase).ToList();

    public static List<string> GetRoomParameterNames(Document doc)
    {
        var room = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rooms)
            .WhereElementIsNotElementType().OfType<Room>().FirstOrDefault(r => r.Area > 0)
            ?? new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType().OfType<Room>().FirstOrDefault();
        return room == null ? new List<string>() : WritableParameterNames(room);
    }

    public static List<string> GetProjectInfoParameterNames(Document doc) =>
        WritableParameterNames(doc.ProjectInformation);

    public static List<Phase> GetPhases(Document doc) => doc.Phases.Cast<Phase>().ToList();

    public static List<RevitLinkInstance> GetLoadedLinks(Document doc) =>
        new FilteredElementCollector(doc).OfClass(typeof(RevitLinkInstance))
            .Cast<RevitLinkInstance>().Where(l => l.GetLinkDocument() != null).ToList();

    public static string LinkDisplayName(RevitLinkInstance link) =>
        link.GetLinkDocument()?.Title ?? link.Name;

    public static OcupacaoResult Run(Document doc, OcupacaoOptions opt, AppLanguage lang)
    {
        var result = new OcupacaoResult { PhaseName = opt.Phase.Name };

        // Origem: modelo local ou vínculo
        Document sourceDoc;
        Transform? transform = null;
        if (opt.Link == null)
        {
            sourceDoc = doc;
            result.SourceName = LocalizedStrings.Get(lang, "SourceLocal");
        }
        else
        {
            sourceDoc = opt.Link.GetLinkDocument()
                ?? throw new UserFacingException(LocalizedStrings.Format(lang, "ErrLinkNotFound", opt.Link.Name));
            transform = opt.Link.GetTotalTransform();
            result.SourceName = opt.Link.Name;
        }

        // Mobiliário cujo nome de família contém o filtro
        var filter = opt.FamilyFilter.Trim();
        var chairs = new FilteredElementCollector(sourceDoc)
            .OfCategory(BuiltInCategory.OST_Furniture)
            .WhereElementIsNotElementType()
            .OfType<FamilyInstance>()
            .Where(fi => fi.Symbol?.Family?.Name?.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
            .ToList();

        result.ChairsFound = chairs.Count;

        // Ambiente de cada cadeira (ambientes sempre do modelo hospedeiro)
        var counts = new Dictionary<ElementId, int>();
        foreach (var chair in chairs)
        {
            var point = GetPoint(chair);
            if (point == null) { result.ChairsOutside++; continue; }
            if (transform != null) point = transform.OfPoint(point);

            Room? room = null;
            try { room = doc.GetRoomAtPoint(point, opt.Phase); } catch { }

            if (room == null) { result.ChairsOutside++; continue; }
            counts[room.Id] = counts.GetValueOrDefault(room.Id) + 1;
        }

        using var tx = new Transaction(doc, "Ocupação - população por ambiente");
        tx.Start();

        // Zera ambientes colocados sem cadeiras para não deixar valores de execuções anteriores
        var allRooms = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .WhereElementIsNotElementType()
            .OfType<Room>()
            .Where(r => r.Area > 0);

        int total = 0;
        foreach (var room in allRooms)
        {
            var count = counts.GetValueOrDefault(room.Id);
            var roomName = $"{room.Number} {room.Name}".Trim();
            var p = room.LookupParameter(opt.RoomParameterName);

            if (p == null)
            {
                if (count > 0)
                    result.Warnings.Add($"{roomName}: " + LocalizedStrings.Format(lang, "ErrRoomParamMissing", opt.RoomParameterName));
                continue;
            }
            if (p.IsReadOnly)
            {
                if (count > 0)
                    result.Warnings.Add($"{roomName}: " + LocalizedStrings.Format(lang, "ErrReadOnly", opt.RoomParameterName));
                continue;
            }

            try
            {
                if (SetCount(p, count))
                {
                    if (count > 0) { total += count; result.RoomsWithChairs++; }
                }
            }
            catch (Exception ex)
            {
                result.Warnings.Add($"{roomName}: " + LocalizedStrings.Format(lang, "ErrWrite", ex.Message));
            }
        }
        result.Total = total;

        // Total nas Informações de Projeto
        var pTotal = doc.ProjectInformation.LookupParameter(opt.TotalParameterName);
        if (pTotal == null)
            result.TotalStatus = LocalizedStrings.Format(lang, "ErrTotalMissing", opt.TotalParameterName);
        else if (pTotal.IsReadOnly)
            result.TotalStatus = LocalizedStrings.Format(lang, "ErrTotalReadOnly", opt.TotalParameterName);
        else
        {
            try
            {
                SetCount(pTotal, total);
                result.TotalStatus = LocalizedStrings.Format(lang, "TotalOk", opt.TotalParameterName);
            }
            catch (Exception ex)
            {
                result.TotalStatus = LocalizedStrings.Format(lang, "ErrWrite", ex.Message);
            }
        }

        tx.Commit();
        return result;
    }

    private static XYZ? GetPoint(FamilyInstance fi)
    {
        return fi.Location switch
        {
            LocationPoint lp => lp.Point,
            LocationCurve lc => lc.Curve.Evaluate(0.5, true),
            _ => null
        };
    }

    private static bool SetCount(Parameter p, int count)
    {
        switch (p.StorageType)
        {
            case StorageType.Integer: return p.Set(count);
            case StorageType.Double: return p.Set((double)count);
            case StorageType.String: return p.Set(count.ToString());
            default: return false;
        }
    }
}
