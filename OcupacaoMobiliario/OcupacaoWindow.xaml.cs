using System.Windows;
using Autodesk.Revit.DB;

namespace BimBlaster.OcupacaoMobiliario;

public partial class OcupacaoWindow : Window
{
    private readonly AppLanguage _lang;
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
        RoomParamLabel.Text = T("RoomParamLabel");
        TotalParamLabel.Text = T("TotalParamLabel");
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

        // Parâmetros graváveis; pré-seleciona os nomes padrão BG_ quando existirem
        Fill(RoomParamCombo, OcupacaoService.GetRoomParameterNames(doc), "BG_Populacao_ambiente");
        Fill(TotalParamCombo, OcupacaoService.GetProjectInfoParameterNames(doc), "BG_Populacao_Total");
    }

    private static void Fill(System.Windows.Controls.ComboBox combo, List<string> names, string preferred)
    {
        foreach (var n in names) combo.Items.Add(n);
        var idx = names.FindIndex(n => n.Equals(preferred, StringComparison.OrdinalIgnoreCase));
        if (idx >= 0) combo.SelectedIndex = idx;
    }

    private string T(string key) => LocalizedStrings.Get(_lang, key);

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
        if (RoomParamCombo.SelectedIndex < 0)
        {
            MessageBox.Show(this, RoomParamCombo.Items.Count == 0 ? T("ErrNoRooms") : T("ErrRoomParamEmpty"),
                T("WindowTitle"), MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (TotalParamCombo.SelectedIndex < 0)
        {
            MessageBox.Show(this, T("ErrTotalParamEmpty"), T("WindowTitle"), MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Options = new OcupacaoOptions
        {
            Link = SourceCombo.SelectedIndex > 0 ? _links[SourceCombo.SelectedIndex - 1] : null,
            FamilyFilter = FilterBox.Text.Trim(),
            Phase = _phases[PhaseCombo.SelectedIndex],
            RoomParameterName = (string)RoomParamCombo.SelectedItem,
            TotalParameterName = (string)TotalParamCombo.SelectedItem
        };
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
