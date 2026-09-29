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
    public string? SharedParameterFile { get; set; }
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
    public const string RoomParameterName = "BG_Populacao_ambiente";
    public const string TotalParameterName = "BG_Populacao_Total";

    private static readonly (string Name, BuiltInCategory Category)[] RequiredParameters =
    {
        (RoomParameterName, BuiltInCategory.OST_Rooms),
        (TotalParameterName, BuiltInCategory.OST_ProjectInformation),
    };

    /// <summary>True se os dois parâmetros já estão vinculados às categorias corretas.</summary>
    public static bool AllParametersBound(Document doc) =>
        RequiredParameters.All(p => IsBound(doc, p.Name, p.Category));

    private static bool IsBound(Document doc, string name, BuiltInCategory category)
    {
        var it = doc.ParameterBindings.ForwardIterator();
        while (it.MoveNext())
        {
            if (it.Key?.Name != name) continue;
            if (it.Current is ElementBinding eb)
                foreach (Category c in eb.Categories)
                    if (c.BuiltInCategory == category) return true;
        }
        return false;
    }

    public static List<Phase> GetPhases(Document doc) => doc.Phases.Cast<Phase>().ToList();

    public static List<RevitLinkInstance> GetLoadedLinks(Document doc) =>
        new FilteredElementCollector(doc).OfClass(typeof(RevitLinkInstance))
            .Cast<RevitLinkInstance>().Where(l => l.GetLinkDocument() != null).ToList();

    public static string LinkDisplayName(RevitLinkInstance link) =>
        link.GetLinkDocument()?.Title ?? link.Name;

    public static OcupacaoResult Run(Document doc, OcupacaoOptions opt, AppLanguage lang)
    {
        var result = new OcupacaoResult { PhaseName = opt.Phase.Name };

        EnsureParameters(doc, opt.SharedParameterFile, lang);

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
            var p = room.LookupParameter(RoomParameterName);

            if (p == null)
            {
                if (count > 0)
                    result.Warnings.Add($"{roomName}: " + LocalizedStrings.Format(lang, "ErrRoomParamMissing", RoomParameterName));
                continue;
            }
            if (p.IsReadOnly)
            {
                if (count > 0)
                    result.Warnings.Add($"{roomName}: " + LocalizedStrings.Format(lang, "ErrReadOnly", RoomParameterName));
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
        var pTotal = doc.ProjectInformation.LookupParameter(TotalParameterName);
        if (pTotal == null)
            result.TotalStatus = LocalizedStrings.Format(lang, "ErrTotalMissing", TotalParameterName);
        else if (pTotal.IsReadOnly)
            result.TotalStatus = LocalizedStrings.Format(lang, "ErrTotalReadOnly", TotalParameterName);
        else
        {
            try
            {
                SetCount(pTotal, total);
                result.TotalStatus = LocalizedStrings.Format(lang, "TotalOk", TotalParameterName);
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

    /// <summary>Vincula os parâmetros compartilhados às categorias necessárias, se ainda não estiverem.</summary>
    private static void EnsureParameters(Document doc, string? sharedFile, AppLanguage lang)
    {
        var pending = RequiredParameters.Where(p => !IsBound(doc, p.Name, p.Category)).ToList();
        if (pending.Count == 0) return;

        var app = doc.Application;
        var previousFile = app.SharedParametersFilename;

        if (string.IsNullOrWhiteSpace(sharedFile) || !File.Exists(sharedFile))
            throw new UserFacingException(LocalizedStrings.Get(lang, "ErrParamFile"));

        try
        {
            app.SharedParametersFilename = sharedFile;
            var defFile = app.OpenSharedParameterFile()
                ?? throw new UserFacingException(LocalizedStrings.Get(lang, "ErrParamNoFile"));

            using var tx = new Transaction(doc, "Ocupação - instalar parâmetros");
            tx.Start();

            foreach (var (name, builtIn) in pending)
            {
                var category = Category.GetCategory(doc, builtIn);

                // Parâmetro já no projeto mas em outra categoria: acrescenta a categoria
                Definition? existing = null;
                InstanceBinding? existingBinding = null;
                var it = doc.ParameterBindings.ForwardIterator();
                while (it.MoveNext())
                {
                    if (it.Key?.Name == name && it.Current is InstanceBinding ib)
                    {
                        existing = it.Key; existingBinding = ib; break;
                    }
                }

                if (existing != null && existingBinding != null)
                {
                    var set = app.Create.NewCategorySet();
                    foreach (Category c in existingBinding.Categories) set.Insert(c);
                    set.Insert(category);
                    doc.ParameterBindings.ReInsert(existing, app.Create.NewInstanceBinding(set), GroupTypeId.Data);
                    continue;
                }

                Definition? def = null;
                foreach (DefinitionGroup group in defFile.Groups)
                {
                    def = group.Definitions.get_Item(name);
                    if (def != null) break;
                }
                if (def == null)
                {
                    tx.RollBack();
                    throw new UserFacingException(LocalizedStrings.Format(lang, "ErrParamNotInFile", name));
                }

                var cats = app.Create.NewCategorySet();
                cats.Insert(category);
                doc.ParameterBindings.Insert(def, app.Create.NewInstanceBinding(cats), GroupTypeId.Data);
            }

            tx.Commit();
        }
        finally
        {
            if (!string.IsNullOrEmpty(previousFile))
            {
                try { app.SharedParametersFilename = previousFile; } catch { }
            }
        }
    }
}
