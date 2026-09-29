using System.Reflection;
using Autodesk.Revit.UI;

namespace BimBlaster.OcupacaoMobiliario;

public class OcupacaoMobiliarioApplication : IExternalApplication
{
    public Result OnStartup(UIControlledApplication application)
    {
        const string panelName = "BIM BLASTER";
        var panel = application.GetRibbonPanels(Tab.AddIns).FirstOrDefault(p => p.Name == panelName)
                    ?? application.CreateRibbonPanel(Tab.AddIns, panelName);

        var lang = application.ControlledApplication.Language;
        var buttonData = new PushButtonData(
            "OcupacaoMobiliario",
            LocalizedStrings.Get(lang, "ButtonText"),
            Assembly.GetExecutingAssembly().Location,
            "BimBlaster.OcupacaoMobiliario.OcupacaoMobiliarioCommand")
        {
            ToolTip = LocalizedStrings.Get(lang, "ButtonTooltip"),
            LongDescription = LocalizedStrings.Get(lang, "ButtonLongDescription"),
            LargeImage = IconLoader.Load("icon32.png"),
            Image = IconLoader.Load("icon16.png")
        };

        panel.AddItem(buttonData);
        return Result.Succeeded;
    }

    public Result OnShutdown(UIControlledApplication application) => Result.Succeeded;
}
