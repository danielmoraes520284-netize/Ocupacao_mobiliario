using Autodesk.Revit.ApplicationServices;

namespace BimBlaster.OcupacaoMobiliario;

public enum AppLanguage { Portuguese, English, Spanish }

public static class LanguageResolver
{
    public static AppLanguage Resolve(LanguageType revitLanguage) => revitLanguage switch
    {
        LanguageType.Brazilian_Portuguese => AppLanguage.Portuguese,
        LanguageType.Spanish => AppLanguage.Spanish,
        LanguageType.English_USA => AppLanguage.English,
        LanguageType.English_GB => AppLanguage.English,
        _ => AppLanguage.Portuguese
    };
}

public static class LocalizedStrings
{
    private static readonly Dictionary<AppLanguage, Dictionary<string, string>> Strings = new()
    {
        [AppLanguage.Portuguese] = new()
        {
            ["RoomParamLabel"] = "Parâmetro de população do ambiente",
            ["TotalParamLabel"] = "Parâmetro de soma (Informações de Projeto)",
            ["ErrRoomParamEmpty"] = "Selecione o parâmetro dos ambientes.",
            ["ErrTotalParamEmpty"] = "Selecione o parâmetro de soma das Informações de Projeto.",
            ["ErrNoRooms"] = "Nenhum ambiente encontrado no modelo.",
            ["ButtonText"] = "Ocupação",
            ["ButtonTooltip"] = "Computa as cadeiras por ambiente e grava a população",
            ["ButtonLongDescription"] = "Conta as famílias de mobiliário cujo nome contém o filtro (padrão CADEIRA), no modelo local ou em um vínculo, grava o resultado no parâmetro escolhido de cada ambiente e o total no parâmetro escolhido das Informações de Projeto.",
            ["WindowTitle"] = "Cômputo de População por Cadeiras",
            ["Subtitle"] = "Conta cadeiras por ambiente e grava a população nos parâmetros compartilhados.",
            ["SourceLabel"] = "Origem do mobiliário (modelo local ou vínculo)",
            ["SourceLocal"] = "(Modelo atual / sem vínculo)",
            ["FilterLabel"] = "Filtro do nome da família",
            ["PhaseLabel"] = "Fase",

            ["Cancel"] = "Cancelar",
            ["Run"] = "Executar",
            ["ErrFilterEmpty"] = "Informe o filtro do nome da família.",
            ["ErrPhaseEmpty"] = "Selecione uma fase.",


            ["ErrLinkNotFound"] = "Vínculo '{0}' não encontrado ou descarregado.",
            ["ErrPhaseNotFound"] = "Fase '{0}' não encontrada no projeto.",
            ["ErrRoomParamMissing"] = "Parâmetro '{0}' não encontrado no ambiente.",
            ["ErrReadOnly"] = "Parâmetro '{0}' é somente leitura.",
            ["ErrWrite"] = "Erro ao escrever: {0}",
            ["ErrTotalMissing"] = "Parâmetro '{0}' não encontrado nas Informações de Projeto.",
            ["ErrTotalReadOnly"] = "Parâmetro '{0}' é somente leitura nas Informações de Projeto.",
            ["ResultTitle"] = "Resultado - Cômputo de População",
            ["ResultBody"] = "Processo concluído.\n\n- População total: {0}\n- Ambientes com cadeiras: {1}\n- Cadeiras encontradas: {2}\n- Cadeiras fora de ambiente / sem localização: {3}\n- Origem analisada: {4}\n- Fase: {5}\n- Informações de Projeto: {6}",
            ["TotalOk"] = "total gravado em '{0}'",
            ["ResultErrors"] = "\n\nAvisos ({0}):\n{1}",
        },
        [AppLanguage.English] = new()
        {
            ["RoomParamLabel"] = "Room population parameter",
            ["TotalParamLabel"] = "Total parameter (Project Information)",
            ["ErrRoomParamEmpty"] = "Select the room parameter.",
            ["ErrTotalParamEmpty"] = "Select the Project Information total parameter.",
            ["ErrNoRooms"] = "No rooms found in the model.",
            ["ButtonText"] = "Occupancy",
            ["ButtonTooltip"] = "Counts chairs per room and writes the population",
            ["ButtonLongDescription"] = "Counts furniture families whose name contains the filter (default CADEIRA), in the local model or in a link, writes the result to the chosen parameter of each room and the total to the chosen Project Information parameter.",
            ["WindowTitle"] = "Chair Population Count",
            ["Subtitle"] = "Counts chairs per room and writes the population to the shared parameters.",
            ["SourceLabel"] = "Furniture source (local model or link)",
            ["SourceLocal"] = "(Current model / no link)",
            ["FilterLabel"] = "Family name filter",
            ["PhaseLabel"] = "Phase",

            ["Cancel"] = "Cancel",
            ["Run"] = "Run",
            ["ErrFilterEmpty"] = "Enter the family name filter.",
            ["ErrPhaseEmpty"] = "Select a phase.",


            ["ErrLinkNotFound"] = "Link '{0}' not found or unloaded.",
            ["ErrPhaseNotFound"] = "Phase '{0}' not found in the project.",
            ["ErrRoomParamMissing"] = "Parameter '{0}' not found on the room.",
            ["ErrReadOnly"] = "Parameter '{0}' is read-only.",
            ["ErrWrite"] = "Write error: {0}",
            ["ErrTotalMissing"] = "Parameter '{0}' not found in Project Information.",
            ["ErrTotalReadOnly"] = "Parameter '{0}' is read-only in Project Information.",
            ["ResultTitle"] = "Result - Population Count",
            ["ResultBody"] = "Process completed.\n\n- Total population: {0}\n- Rooms with chairs: {1}\n- Chairs found: {2}\n- Chairs outside rooms / without location: {3}\n- Source analyzed: {4}\n- Phase: {5}\n- Project Information: {6}",
            ["TotalOk"] = "total written to '{0}'",
            ["ResultErrors"] = "\n\nWarnings ({0}):\n{1}",
        },
        [AppLanguage.Spanish] = new()
        {
            ["RoomParamLabel"] = "Parámetro de población del ambiente",
            ["TotalParamLabel"] = "Parámetro de suma (Información del proyecto)",
            ["ErrRoomParamEmpty"] = "Seleccione el parámetro de los ambientes.",
            ["ErrTotalParamEmpty"] = "Seleccione el parámetro de suma de Información del proyecto.",
            ["ErrNoRooms"] = "No se encontraron ambientes en el modelo.",
            ["ButtonText"] = "Ocupación",
            ["ButtonTooltip"] = "Cuenta las sillas por ambiente y escribe la población",
            ["ButtonLongDescription"] = "Cuenta las familias de mobiliario cuyo nombre contiene el filtro (por defecto CADEIRA), en el modelo local o en un vínculo, escribe el resultado en el parámetro elegido de cada ambiente y el total en el parámetro elegido de Información del proyecto.",
            ["WindowTitle"] = "Cómputo de Población por Sillas",
            ["Subtitle"] = "Cuenta sillas por ambiente y escribe la población en los parámetros compartidos.",
            ["SourceLabel"] = "Origen del mobiliario (modelo local o vínculo)",
            ["SourceLocal"] = "(Modelo actual / sin vínculo)",
            ["FilterLabel"] = "Filtro del nombre de la familia",
            ["PhaseLabel"] = "Fase",

            ["Cancel"] = "Cancelar",
            ["Run"] = "Ejecutar",
            ["ErrFilterEmpty"] = "Indique el filtro del nombre de la familia.",
            ["ErrPhaseEmpty"] = "Seleccione una fase.",


            ["ErrLinkNotFound"] = "Vínculo '{0}' no encontrado o descargado.",
            ["ErrPhaseNotFound"] = "Fase '{0}' no encontrada en el proyecto.",
            ["ErrRoomParamMissing"] = "Parámetro '{0}' no encontrado en el ambiente.",
            ["ErrReadOnly"] = "El parámetro '{0}' es de solo lectura.",
            ["ErrWrite"] = "Error al escribir: {0}",
            ["ErrTotalMissing"] = "Parámetro '{0}' no encontrado en Información del proyecto.",
            ["ErrTotalReadOnly"] = "El parámetro '{0}' es de solo lectura en Información del proyecto.",
            ["ResultTitle"] = "Resultado - Cómputo de Población",
            ["ResultBody"] = "Proceso concluido.\n\n- Población total: {0}\n- Ambientes con sillas: {1}\n- Sillas encontradas: {2}\n- Sillas fuera de ambiente / sin ubicación: {3}\n- Origen analizado: {4}\n- Fase: {5}\n- Información del proyecto: {6}",
            ["TotalOk"] = "total escrito en '{0}'",
            ["ResultErrors"] = "\n\nAvisos ({0}):\n{1}",
        },
    };

    public static string Get(LanguageType revitLanguage, string key) =>
        Get(LanguageResolver.Resolve(revitLanguage), key);

    public static string Get(AppLanguage language, string key) =>
        Strings[language].GetValueOrDefault(key, Strings[AppLanguage.Portuguese][key]);

    public static string Format(AppLanguage language, string key, params object[] args) =>
        string.Format(Get(language, key), args);
}
