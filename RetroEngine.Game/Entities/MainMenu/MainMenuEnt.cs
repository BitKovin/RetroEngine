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

                menuButtons.Origin = new Vector2(0,-0.5f);

                AddChild(menuButtons);

            }

            public override void Update()
            {
                base.Update();

                Console.WriteLine(Input.MousePos);

            }

        }

        class MenuButtons : UiCanvas
        {
            public MenuButtons()
            {
                Button button = new Button();
                button.size = new Vector2(100, 20);
                button.position = new Vector2(0, 0);
                AddChild(button);

                Button button2 = new Button();
                button2.size = new Vector2(100, 20);
                button2.position = new Vector2(0, 30);
                AddChild(button2);
            }

            public override void Update()
            {
                base.Update();
            }

        }

    }

    

}
