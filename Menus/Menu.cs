using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Miner_Of_Duty.Menus
{
    public class Menu : IMenuOwner
    {
        public delegate void MenuOptionChosen(IMenuOwner sender, string id);
        public delegate void BackPressed(object sender);
        public delegate void SelectedIndexChanged(Menu sender);
        public event SelectedIndexChanged SelectedIndexChangedEvent;

        private MenuOptionChosen optionChose;
        private BackPressed backPressed;
        private MenuElement[] elements;
        private int selectedElement;
        private int minElement;
        private int delay; //used for selecting

        public Menu(MenuOptionChosen mocDelegate, BackPressed backDelegate, MenuElement[] elements)
        {
            optionChose = mocDelegate;
            backPressed = backDelegate;
            selectedElement = 0;
            minElement = 0;

            Vector2 textStartPos = new Vector2(125, 90);
            float textHeight = Resources.Font.LineSpacing * 1.15f;

            for (int i = 0; i < elements.Length; i++)
            {
                textStartPos.Y += textHeight;
                elements[i].Position = textStartPos;
            }

            this.elements = elements;
        }

        public Menu(MenuOptionChosen mocDelegate, BackPressed backDelegate, MenuElement[] elements, float offset)
        {
            optionChose = mocDelegate;
            backPressed = backDelegate;
            selectedElement = 0;
            minElement = 0;

            Vector2 textStartPos = new Vector2(125 + offset, 90);
            float textHeight = Resources.Font.LineSpacing * 1.15f;

            for (int i = 0; i < elements.Length; i++)
            {
                textStartPos.Y += textHeight;
                elements[i].Position = textStartPos;
            }

            this.elements = elements;
        }

        public Menu(MenuOptionChosen mocDelegate, BackPressed backDelegate, MenuElement[] elements, float xOffset, float yOffset)
        {
            optionChose = mocDelegate;
            backPressed = backDelegate;
            selectedElement = 0;
            minElement = 0;

            Vector2 textStartPos = new Vector2(125 + xOffset, 90 + yOffset);
            float textHeight = Resources.Font.LineSpacing * 1.15f;

            for (int i = 0; i < elements.Length; i++)
            {
                textStartPos.Y += textHeight;
                elements[i].Position = textStartPos;
            }

            this.elements = elements;
        }

        public Menu(MenuOptionChosen mocDelegate, BackPressed backDelegate, MenuElement[] elements, float offset, int minSelected)
        {
            optionChose = mocDelegate;
            backPressed = backDelegate;
            selectedElement = minSelected;
            minElement = minSelected;

            Vector2 textStartPos = new Vector2(125 + offset, 90);
            float textHeight = Resources.Font.LineSpacing * 1.15f;

            for (int i = 0; i < elements.Length; i++)
            {
                textStartPos.Y += textHeight;
                elements[i].Position = textStartPos;
            }

            this.elements = elements;
        }

        public void SelectFirst()
        {
            selectedElement = minElement;

            if (SelectedIndexChangedEvent != null)
                SelectedIndexChangedEvent.Invoke(this);
        }

        public string GetSelectedItemID()
        {
            return elements[selectedElement].ID;
        }

        public MenuElement this[string elementID]
        {
            get
            {
                for (int i = 0; i < elements.Length; i++)
                {
                    if (elements[i].ID == elementID)
                        return elements[i];
                }
                return null;
            }
        }

        public void Update(short timePassedInMilliseconds)
        {
            if (Input.WasButtonPressed(Microsoft.Xna.Framework.Input.Buttons.A))
            {
                if (elements[selectedElement].CanSelect())
                {
                    optionChose.Invoke(this, elements[selectedElement].ID);
                    Audio.PlaySound(Audio.SOUND_UICLICK);
                }
                else
                {
                    Audio.PlaySound(Audio.SOUND_UIERROR);
                }
            }
            else if (Input.WasButtonPressed(Microsoft.Xna.Framework.Input.Buttons.B) || Input.WasButtonPressed(Microsoft.Xna.Framework.Input.Buttons.Back))
            {
                backPressed.Invoke(this);
                Audio.PlaySound(Audio.SOUND_UICLICK);
            }

            if (delay <= 0)
            {
                if (Input.IsThumbstickOrDPad(Input.Direction.Up))
                {
                    if (selectedElement - 1 >= minElement)
                    {
                        selectedElement--;
                        if (SelectedIndexChangedEvent != null)
                            SelectedIndexChangedEvent.Invoke(this);
                        delay = 200;
                    }
                }
                else if (Input.IsThumbstickOrDPad(Input.Direction.Down))
                {
                    if (selectedElement + 1 < elements.Length)
                    {
                        selectedElement++;
                        delay = 200;
                        if (SelectedIndexChangedEvent != null)
                            SelectedIndexChangedEvent.Invoke(this);
                    }
                }
            }
            else
                delay -= timePassedInMilliseconds;

            for (int i = 0; i < elements.Length; i++)
            {
                if (elements[i] is ValueElement)
                {
                    if (i == selectedElement && (Input.IsThumbstickOrDPad(Input.Direction.Left) || Input.IsThumbstickOrDPad(Input.Direction.Right)))
                    {
                        (elements[i] as ValueElement).Update(
                            Input.IsThumbstickOrDPad(Input.Direction.Left) ? -.5f : .5f, timePassedInMilliseconds);
                    }
                    else if (i == selectedElement)
                    {
                        (elements[i] as ValueElement).Update(0, timePassedInMilliseconds);
                    }
                    else
                        (elements[i] as ValueElement).Update(timePassedInMilliseconds);
                }
            }

        }

        public void Draw(SpriteBatch sb)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                elements[i].Draw(sb, i == selectedElement);
            }
        }
    }
}
