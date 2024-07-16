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

                MenuButtons menuButtons = new MenuButtons();

                menuButtons.Origin = new Vector2(0.0f,0.5f);
                menuButtons.position = new Vector2(10,0);

                //position = new Vector2(300,300);

                AddChild(menuButtons);

            }

        }

        class MenuButtons : UiCanvas
        {
            public MenuButtons()
            {

                //DrawBorder = true;

                Button button = new Button();
                button.size = new Vector2(200, 50);
                button.position = new Vector2(0, 0);
                
                Text text = new Text();
                text.text = "Play";
                text.Pivot = new Vector2(0.5f);
                text.Origin = new Vector2(0.5f);
                text.baseColor = Color.Black;
                button.AddChild(text);

                AddChild(button);

                Button button2 = new Button();
                button2.size = new Vector2(100, 20);
                button2.position = new Vector2(0, 100);
                AddChild(button2);
            }

            public override void Update()
            {
                base.Update();
            }

        }

    }

    

}
