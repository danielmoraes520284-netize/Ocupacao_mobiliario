# Ocupação Mobiliário (BIMBLASTER)

Plugin Revit 2025/2026. Conta as famílias de mobiliário cujo nome contém "CADEIRA" (filtro editável), no modelo local ou em um vínculo, por ambiente e fase:

- grava a contagem em `BG_Populacao_ambiente` (parâmetro compartilhado dos Ambientes);
- grava a soma em `BG_Populacao_Total` (parâmetro compartilhado das Informações de Projeto).

Botão: aba Complementos > painel BIM BLASTER > Ocupação.

Instalação: copie o conteúdo de `dist/` para `%AppData%\Autodesk\Revit\Addins\2025\` (e/ou `2026`).
