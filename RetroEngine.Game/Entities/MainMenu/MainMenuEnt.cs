using Microsoft.Xna.Framework;
using RetroEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Game.Entities.MainMenu
{

    [LevelObject("main_menu")]
    internal class MainMenuEnt : Entity
    {

        public override void Start()
        {
            base.Start();

            UiElement.Viewport.AddChild(new MainMenuUi());

            Input.LockCursor = false;

        }

        class MainMenuUi : UiCanvas
        {

            public MainMenuUi()
            {

                AssetRegistry.LoadFmodBankIntoMemory("sounds/banks/Music.bank");

                MenuButtons menuButtons = new MenuButtons();

                menuButtons.Origin = new Vector2(0.0f,0.5f);
                menuButtons.position = new Vector2(10,0);

                //position = new Vector2(300,300);

                AddChild(menuButtons);

            }

        }

        class MenuButtons : ContentBox
        {
            public MenuButtons()
            {

                Pivot = new Vector2(0, 0.5f);
                

                DrawBorder = true;

                MenuButton button = new MenuButton();
                button.size = new Vector2(500, 100);
                button.position = new Vector2(0, 0);
                button.Text = "Play";
                button.uiText.FontSize = 52;
                button.onReleased += () => { Level.LoadLevelFromFile("lvl1"); };
                AddChild(button);

                MenuButton button2 = new MenuButton();
                button2.size = new Vector2(500, 100);
                button2.position = new Vector2(0, 150);
                button2.Text = "Quit";
                button2.uiText.FontSize = 52;
                button2.onReleased += () => { System.Environment.Exit(0); };
                AddChild(button2);
            }

            public override void Update()
            {
                base.Update();
            }

            class MenuButton : Button
            {

                public UiText uiText;

                public string Text { get { return uiText.text.ToString(); } set { uiText.text = value; } }

                public MenuButton() : base()
                {
                    uiText = new UiText();
                    uiText.text = "hello world";

                    uiText.Origin = new Vector2(0.5f);
                    uiText.Pivot = new Vector2(0.5f);
                    uiText.baseColor = Color.Black;

                    AddChild(uiText);

                }

            }

        }

    }

    

}
