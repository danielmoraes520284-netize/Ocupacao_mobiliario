using System.Windows;
using Autodesk.Revit.DB;
using Microsoft.Win32;

namespace BimBlaster.OcupacaoMobiliario;

public partial class OcupacaoWindow : Window
{
    private readonly AppLanguage _lang;
    private readonly bool _paramsMissing;
    private readonly List<RevitLinkInstance> _links;
    private readonly List<Phase> _phases;

    public OcupacaoOptions? Options { get; private set; }

    public OcupacaoWindow(Document doc, AppLanguage lang)
    {
        _lang = lang;
        // O dicionário precisa entrar em Resources antes de InitializeComponent()
        Resources.MergedDictionaries.Add(StyleLoader.LoadBimBlasterStyles());
        InitializeComponent();

        Title = T("WindowTitle");
        TitleText.Text = T("WindowTitle");
        SubtitleText.Text = T("Subtitle");
        SourceLabel.Text = T("SourceLabel");
        FilterLabel.Text = T("FilterLabel");
        PhaseLabel.Text = T("PhaseLabel");
        ParamFileLabel.Text = T("ParamFileLabel");
        BrowseButton.Content = T("Browse");
        CancelButton.Content = T("Cancel");
        RunButton.Content = T("Run");

        // Origem: modelo local + vínculos carregados
        _links = OcupacaoService.GetLoadedLinks(doc);
        SourceCombo.Items.Add(T("SourceLocal"));
        foreach (var l in _links) SourceCombo.Items.Add(OcupacaoService.LinkDisplayName(l));
        SourceCombo.SelectedIndex = 0;

        // Fases (padrão: "Novo"/"New"/"Nuevo", senão a última)
        _phases = OcupacaoService.GetPhases(doc);
        foreach (var p in _phases) PhaseCombo.Items.Add(p.Name);
        var defaultIdx = _phases.FindIndex(p =>
            new[] { "novo", "nova", "new", "nuevo", "nueva" }.Contains(p.Name.Trim().ToLowerInvariant()));
        if (defaultIdx < 0) defaultIdx = _phases.Count - 1;
        if (defaultIdx >= 0) PhaseCombo.SelectedIndex = defaultIdx;

        // Arquivo de parâmetros: só é obrigatório se os parâmetros ainda não existirem
        _paramsMissing = !OcupacaoService.AllParametersBound(doc);
        ParamHint.Text = T(_paramsMissing ? "ParamFileHintMissing" : "ParamFileHintOk");
        var current = doc.Application.SharedParametersFilename;
        if (!string.IsNullOrWhiteSpace(current) && File.Exists(current))
            ParamFileBox.Text = current;
    }

    private string T(string key) => LocalizedStrings.Get(_lang, key);

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Title = T("BrowseTitle"),
            Filter = "TXT (*.txt)|*.txt",
            CheckFileExists = true
        };
        if (dlg.ShowDialog(this) == true) ParamFileBox.Text = dlg.FileName;
    }

    private void Run_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(FilterBox.Text))
        {
            MessageBox.Show(this, T("ErrFilterEmpty"), T("WindowTitle"), MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (PhaseCombo.SelectedIndex < 0)
        {
            MessageBox.Show(this, T("ErrPhaseEmpty"), T("WindowTitle"), MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (_paramsMissing && !File.Exists(ParamFileBox.Text))
        {
            MessageBox.Show(this, T("ErrParamFile"), T("WindowTitle"), MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Options = new OcupacaoOptions
        {
            Link = SourceCombo.SelectedIndex > 0 ? _links[SourceCombo.SelectedIndex - 1] : null,
            FamilyFilter = FilterBox.Text.Trim(),
            Phase = _phases[PhaseCombo.SelectedIndex],
            SharedParameterFile = ParamFileBox.Text
        };
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
