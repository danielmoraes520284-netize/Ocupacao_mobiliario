using System.Windows.Interop;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BimBlaster.OcupacaoMobiliario;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class OcupacaoMobiliarioCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var uiApp = commandData.Application;
        var doc = uiApp.ActiveUIDocument.Document;
        var lang = LanguageResolver.Resolve(uiApp.Application.Language);

        try
        {
            var window = new OcupacaoWindow(doc, lang);
            new WindowInteropHelper(window).Owner = uiApp.MainWindowHandle;
            if (window.ShowDialog() != true || window.Options == null)
                return Result.Cancelled;

            var result = OcupacaoService.Run(doc, window.Options, lang);
            TaskDialog.Show(LocalizedStrings.Get(lang, "ResultTitle"), result.ToMessage(lang));
            return Result.Succeeded;
        }
        catch (UserFacingException ex)
        {
            TaskDialog.Show(LocalizedStrings.Get(lang, "WindowTitle"), ex.Message);
            return Result.Failed;
        }
        catch (Exception ex)
        {
            message = ex.Message;
            return Result.Failed;
        }
    }
}
