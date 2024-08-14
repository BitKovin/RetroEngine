using System;
using System.Runtime.Versioning;
using System.Windows.Forms;
using MonoGame.Extended.WinForms;
using Microsoft.Xna.Framework;

namespace RetroEngine.WinForms.WindowsDX;

[SupportedOSPlatform("windows7.0")]
public partial class Form1 : Form
{

    public Form1(GameMain game)
    {

        game1 = game;

        InitializeComponent();
        RegisterEventHandlers();
    }

    public int BottomHitCount { get; private set; }

    public event EventHandler BottomHitCountChanged;

    protected override bool ProcessKeyPreview(ref Message m)
    {
        
        gameControl.ProcessKeyMessage(ref m, true);
        return base.ProcessKeyPreview(ref m);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        gameControl.ProcessKeyMessage(ref msg, true);

        if (checkBox1.Checked)
        {
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void RegisterEventHandlers()
    {

        var game = new Microsoft.Xna.Framework.Game();

        gameControl.GameInitialize += GameControl_GameInitialize;
        gameControl.GameLoadContents += GameControl_GameLoadContents;
        gameControl.GameUnloadContents += GameControl_GameUnloadContents;
        gameControl.GameUpdate += GameControl_GameUpdate;
        gameControl.GameDraw += GameControl_GameDraw;
        gameControl.GameDisposed += GameControl_GameDisposed;

        BottomHitCountChanged += HandleBottomHitCountChanged;
        numericUpDown1.ValueChanged += numericUpDown1_ValueChanged;
    }

    private void GameControl_GameDisposed(object sender, EventArgs e)
    {
    }

    private void GameControl_GameDraw(object sender, TimedEventArgs e)
    {
        game1.DoDraw(e.GameTime);
    }

    private void GameControl_GameUpdate(object sender, TimedEventArgs e)
    {
        game1.DoUpdate(e.GameTime);

    }

    private void GameControl_GameUnloadContents(object sender, EventArgs e)
    {
        game1.DoUnloadContent();
    }

    private void GameControl_GameLoadContents(object sender, EventArgs e)
    {
        game1.DoLoadContent();
    }

    private void GameControl_GameInitialize(object sender, EventArgs e)
    {
        //InputManager.Initialize(new ControlInputManager());
        lock(game1)
        game1.DoGameInitialized();
    }

    private void HandleBottomHitCountChanged(object sender, EventArgs e)
    {
        textBox1.Text = BottomHitCount.ToString();
    }

    private void numericUpDown1_ValueChanged(object sender, EventArgs e)
    {
    }

    private void OnBottomHitCountChanged(EventArgs e)
    {
        BottomHitCountChanged?.Invoke(this, e);
    }

    private GameMain game1 = null;

}
