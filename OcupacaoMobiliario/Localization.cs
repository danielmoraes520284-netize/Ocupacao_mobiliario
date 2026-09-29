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
            ["ButtonText"] = "Ocupação",
            ["ButtonTooltip"] = "Computa as cadeiras por ambiente e grava a população",
            ["ButtonLongDescription"] = "Conta as famílias de mobiliário cujo nome contém o filtro (padrão CADEIRA), no modelo local ou em um vínculo, grava o resultado em BG_Populacao_ambiente em cada ambiente e o total em BG_Populacao_Total nas Informações de Projeto.",
            ["WindowTitle"] = "Cômputo de População por Cadeiras",
            ["Subtitle"] = "Conta cadeiras por ambiente e grava a população nos parâmetros compartilhados.",
            ["SourceLabel"] = "Origem do mobiliário (modelo local ou vínculo)",
            ["SourceLocal"] = "(Modelo atual / sem vínculo)",
            ["FilterLabel"] = "Filtro do nome da família",
            ["PhaseLabel"] = "Fase",
            ["ParamFileLabel"] = "Arquivo de parâmetros compartilhados (.txt)",
            ["ParamFileHintMissing"] = "Parâmetros ausentes no projeto: informe o arquivo para instalá-los.",
            ["ParamFileHintOk"] = "Opcional: os parâmetros já existem no projeto.",
            ["Browse"] = "Procurar...",
            ["BrowseTitle"] = "Selecione o arquivo de parâmetros compartilhados",
            ["Cancel"] = "Cancelar",
            ["Run"] = "Executar",
            ["ErrFilterEmpty"] = "Informe o filtro do nome da família.",
            ["ErrPhaseEmpty"] = "Selecione uma fase.",
            ["ErrParamFile"] = "Parâmetros ausentes no projeto. Selecione um arquivo de parâmetros compartilhados (.txt) válido.",
            ["ErrParamNotInFile"] = "Parâmetro '{0}' não encontrado no arquivo de parâmetros compartilhados.",
            ["ErrParamNoFile"] = "Não foi possível abrir o arquivo de parâmetros compartilhados.",
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
            ["ButtonText"] = "Occupancy",
            ["ButtonTooltip"] = "Counts chairs per room and writes the population",
            ["ButtonLongDescription"] = "Counts furniture families whose name contains the filter (default CADEIRA), in the local model or in a link, writes the result to BG_Populacao_ambiente on each room and the total to BG_Populacao_Total in Project Information.",
            ["WindowTitle"] = "Chair Population Count",
            ["Subtitle"] = "Counts chairs per room and writes the population to the shared parameters.",
            ["SourceLabel"] = "Furniture source (local model or link)",
            ["SourceLocal"] = "(Current model / no link)",
            ["FilterLabel"] = "Family name filter",
            ["PhaseLabel"] = "Phase",
            ["ParamFileLabel"] = "Shared parameters file (.txt)",
            ["ParamFileHintMissing"] = "Parameters missing in the project: provide the file to install them.",
            ["ParamFileHintOk"] = "Optional: the parameters already exist in the project.",
            ["Browse"] = "Browse...",
            ["BrowseTitle"] = "Select the shared parameters file",
            ["Cancel"] = "Cancel",
            ["Run"] = "Run",
            ["ErrFilterEmpty"] = "Enter the family name filter.",
            ["ErrPhaseEmpty"] = "Select a phase.",
            ["ErrParamFile"] = "Parameters are missing in the project. Select a valid shared parameters (.txt) file.",
            ["ErrParamNotInFile"] = "Parameter '{0}' not found in the shared parameters file.",
            ["ErrParamNoFile"] = "Could not open the shared parameters file.",
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
            ["ButtonText"] = "Ocupación",
            ["ButtonTooltip"] = "Cuenta las sillas por ambiente y escribe la población",
            ["ButtonLongDescription"] = "Cuenta las familias de mobiliario cuyo nombre contiene el filtro (por defecto CADEIRA), en el modelo local o en un vínculo, escribe el resultado en BG_Populacao_ambiente en cada ambiente y el total en BG_Populacao_Total en Información del proyecto.",
            ["WindowTitle"] = "Cómputo de Población por Sillas",
            ["Subtitle"] = "Cuenta sillas por ambiente y escribe la población en los parámetros compartidos.",
            ["SourceLabel"] = "Origen del mobiliario (modelo local o vínculo)",
            ["SourceLocal"] = "(Modelo actual / sin vínculo)",
            ["FilterLabel"] = "Filtro del nombre de la familia",
            ["PhaseLabel"] = "Fase",
            ["ParamFileLabel"] = "Archivo de parámetros compartidos (.txt)",
            ["ParamFileHintMissing"] = "Parámetros ausentes en el proyecto: indique el archivo para instalarlos.",
            ["ParamFileHintOk"] = "Opcional: los parámetros ya existen en el proyecto.",
            ["Browse"] = "Buscar...",
            ["BrowseTitle"] = "Seleccione el archivo de parámetros compartidos",
            ["Cancel"] = "Cancelar",
            ["Run"] = "Ejecutar",
            ["ErrFilterEmpty"] = "Indique el filtro del nombre de la familia.",
            ["ErrPhaseEmpty"] = "Seleccione una fase.",
            ["ErrParamFile"] = "Faltan parámetros en el proyecto. Seleccione un archivo de parámetros compartidos (.txt) válido.",
            ["ErrParamNotInFile"] = "Parámetro '{0}' no encontrado en el archivo de parámetros compartidos.",
            ["ErrParamNoFile"] = "No se pudo abrir el archivo de parámetros compartidos.",
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
