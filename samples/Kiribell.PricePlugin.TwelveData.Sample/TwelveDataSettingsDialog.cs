using System.Text.Json;
using Forms = System.Windows.Forms;

namespace Kiribell.PricePlugin.TwelveData.Sample;

internal sealed class TwelveDataSettingsDialog : Forms.Form
{
    private readonly Forms.TextBox _apiKey = new() { Dock = Forms.DockStyle.Top, UseSystemPasswordChar = true };
    public string? ConfigurationJson { get; private set; }

    public TwelveDataSettingsDialog(string? configurationJson)
    {
        Text = "Twelve Data プラグイン設定";
        StartPosition = Forms.FormStartPosition.CenterParent;
        FormBorderStyle = Forms.FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new System.Drawing.Size(430, 175);
        try { _apiKey.Text = JsonSerializer.Deserialize<TwelveDataConfiguration>(configurationJson ?? "")?.ApiKey ?? ""; }
        catch (JsonException) { }

        var label = new Forms.Label { AutoSize = true, Dock = Forms.DockStyle.Top, Text = "Twelve Data APIキー" };
        var description = new Forms.Label
        {
            AutoSize = true,
            Dock = Forms.DockStyle.Top,
            MaximumSize = new System.Drawing.Size(406, 0),
            Padding = new Forms.Padding(0, 0, 0, 12),
            Text = "キーはキリベルのプラグイン設定として、このPCのユーザー設定に保存されます。"
        };
        var cancel = new Forms.Button { Text = "キャンセル", DialogResult = Forms.DialogResult.Cancel, AutoSize = true };
        var ok = new Forms.Button { Text = "保存", AutoSize = true };
        ok.Click += (_, _) => Save();
        var buttons = new Forms.FlowLayoutPanel
        {
            Dock = Forms.DockStyle.Bottom,
            FlowDirection = Forms.FlowDirection.RightToLeft,
            AutoSize = true,
            Padding = new Forms.Padding(0, 10, 0, 0)
        };
        buttons.Controls.Add(cancel);
        buttons.Controls.Add(ok);
        var content = new Forms.Panel { Dock = Forms.DockStyle.Fill, Padding = new Forms.Padding(12) };
        content.Controls.Add(_apiKey);
        content.Controls.Add(label);
        content.Controls.Add(description);
        Controls.Add(content);
        Controls.Add(buttons);
        AcceptButton = ok;
        CancelButton = cancel;
    }

    private void Save()
    {
        var apiKey = _apiKey.Text.Trim();
        if (apiKey.Length == 0)
        {
            Forms.MessageBox.Show(this, "Twelve Data APIキーを入力してください。", Text, Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Warning);
            return;
        }
        ConfigurationJson = JsonSerializer.Serialize(new TwelveDataConfiguration(apiKey));
        DialogResult = Forms.DialogResult.OK;
        Close();
    }
}

internal sealed class WindowOwner(nint handle) : Forms.IWin32Window
{
    public nint Handle => handle;
}
