using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Miner_Of_Duty.Game;

namespace Miner_Of_Duty.Menus
{
    public class MenuElement
    {
        public Vector2 Position;
        public string ID;
        public string Text;
        public Color NormalTextColor;
        public Color SelectedTextColor;

        public MenuElement(string id, string text)
        {
            ID = id;
            Text = text.ToUpper();
            NormalTextColor = Color.White;
            SelectedTextColor = Color.Green;
        }

        public MenuElement(string id, string text, Color normalTextColor, Color selectedTextColor)
        {
            ID = id;
            Text = text.ToUpper();
            NormalTextColor = normalTextColor;
            SelectedTextColor = selectedTextColor;
        }

        public bool CanSelectMe = true;
        public virtual bool CanSelect()
        {
            return CanSelectMe;
        }

        protected virtual Color GetColor(bool isSelected)
        {
            if (isSelected)
            {
                return SelectedTextColor;
            }
            else
            {
                return NormalTextColor;
            }
        }

        public void Draw(SpriteBatch sb, bool selected)
        {
            if (Text == null)
                Text = "null (ERROR!)";
            sb.DrawString(Resources.Font, Text, Position, GetColor(selected));
        }
    }

    public class CostMenuElement : MenuElement
    {
        public IHasCash CashOwner;
        public bool UseExtraCash;
        private string orginalText;
        private int cost;
        public bool Grayed = false;

        public void SetCost(int newCost)
        {
            cost = newCost;
            Text = orginalText + " - $" + cost;
        }

        public CostMenuElement(string id, string text, int cost, IHasCash cashOwner, bool useExtraCash)
            : base(id, text + " - $" + cost)
        {
            orginalText = text.ToUpper();
            this.cost = cost;
            UseExtraCash = useExtraCash;
            CashOwner = cashOwner;
        }

        public override bool CanSelect()
        {
            if (Grayed)
                return false;

            if (UseExtraCash)
            {
                if (cost <= CashOwner.Cash + CashOwner.ExtraCash)
                    return true;
                else
                    return false;
            }
            else
            {
                if (cost <= CashOwner.Cash)
                    return true;
                else
                    return false;
            }
        }

        protected override Color GetColor(bool isSelected)
        {
            if (Grayed)
                if (isSelected)
                    return Color.DarkRed;
                else
                    return Color.Gray;


            if (isSelected)
            {
                if (UseExtraCash)
                {
                    if (cost <= CashOwner.Cash + CashOwner.ExtraCash)
                        return Color.Green;
                    else
                        return Color.Red;
                }
                else
                {
                    if (cost <= CashOwner.Cash)
                        return Color.Green;
                    else
                        return Color.Red;
                }
            }
            else
            {
                if (UseExtraCash)
                {
                    if (cost <= CashOwner.Cash + CashOwner.ExtraCash)
                        return Color.LightGreen;
                    else
                        return Color.Pink;
                }
                else
                {
                    if (cost <= CashOwner.Cash)
                        return Color.LightGreen;
                    else
                        return Color.Pink;
                }
            }
        }

    }

    public class UnlockableMenuElement : MenuElement
    {
        private int UnlockLevel;

        public UnlockableMenuElement(string id, string text, int unlockLevel)
            : base(id, text)
        {
            UnlockLevel = unlockLevel;
        }

        public override bool CanSelect()
        {
            if (UnlockLevel > MinerOfDuty.CurrentPlayerProfile.Level)
                return false;
            else
                return true;
        }

        protected override Color GetColor(bool isSelected)
        {
            if (isSelected)
            {
                if (UnlockLevel > MinerOfDuty.CurrentPlayerProfile.Level)//hasnt been unlocked
                    return Color.DarkRed;
                else
                    return SelectedTextColor;
            }
            else
            {
                if (UnlockLevel > MinerOfDuty.CurrentPlayerProfile.Level)//hasnt been unlocked
                    return Color.Gray;
                else
                    return NormalTextColor;
            }
        }
    }

    public class GamerLockedMenuElement : MenuElement
    {
        public delegate bool IsAvailable();
        private IsAvailable isA;

        public GamerLockedMenuElement(string id, string text, IsAvailable isA)
            : base(id, text)
        {
            this.isA = isA;
        }

        public override bool CanSelect()
        {
            return isA.Invoke();
        }

        protected override Color GetColor(bool isSelected)
        {
            if (isSelected)
            {
                if (isA.Invoke() == false)//hasnt been unlocked
                    return Color.DarkRed;
                else
                    return SelectedTextColor;
            }
            else
            {
                if (isA.Invoke() == false)//hasnt been unlocked
                    return Color.Gray;
                else
                    return NormalTextColor;
            }
        }
    }

    public class LockedMenuElement : UnlockableMenuElement
    {

        public LockedMenuElement(string id, string text)
            : base(id, text, 99999)
        {
        }

        public override bool CanSelect()
        {
            return false;
        }

    }

    public interface ValueElement
    {
        void Update(short timeInMilliseconds);
        void Update(float amount, short timeInMilliseconds);
    }

    public class ValueMenuElement : MenuElement, ValueElement
    {
        public delegate void ValueChanged(string id);
        public event ValueChanged ValueChangedEvent;

        private string originalText, addonText;
        private float val;
        public float Value
        {
            get { return val; }
            set
            {
                val = value;
                Text = originalText + " " + ((int)val).ToString() + addonText;
            }
        }
        private int changeDelay;
        private int minVal, maxVal;
        private int delay;

        public override bool CanSelect()
        {
            return CanSelectMe;
        }

        public ValueMenuElement(string id, string text, float value, string addonText,
            int minVal, int maxVal, int changeDelay)
            : base(id, text)
        {
            originalText = text.ToUpper();
            this.addonText = addonText;
            Value = value;

            this.minVal = minVal;
            this.maxVal = maxVal;
            this.changeDelay = changeDelay;
        }

        public void Update(short timeInMilliseconds)
        {
            if (delay > 0)
                delay -= timeInMilliseconds;
        }

        public void Update(float amount, short timeInMilliseconds)
        {
            Update(timeInMilliseconds);

            if (amount == 0)
                return;

            if (CanSelect() == false)
                return;

            if (delay <= 0)
            {
                float oldVal = val;
                Value = MathHelper.Clamp(Value + (amount * ((float)timeInMilliseconds / 20f)),
                    minVal, maxVal);
                if (oldVal != val)
                {
                    if (ValueChangedEvent != null)
                        ValueChangedEvent.Invoke(ID);
                }

                delay = changeDelay;
            }
        }

    }

    public class BooleanValueMenuElement : MenuElement, ValueElement
    {
        public delegate void ValueChanged(string id);
        public event ValueChanged ValueChangedEvent;

        private string originalText;
        private bool val;
        public bool Value
        {
            get { return val; }
            set
            {
                val = value;
                Text = originalText + " " + val.ToString().ToUpper();
            }
        }
        private int changeDelay;
        private int delay;

        public BooleanValueMenuElement(string id, string text, bool value, int changeDelay)
            : base(id, text)
        {
            originalText = text.ToUpper();
            Value = value;

            this.changeDelay = changeDelay;
        }

        public void Update(short timeInMilliseconds)
        {
            
            if (delay > 0)
                delay -= timeInMilliseconds;

            
        }

        public void Update(float amount, short timeInMilliseconds)
        {
            Update(timeInMilliseconds);

            if (Input.WasButtonPressed(Microsoft.Xna.Framework.Input.Buttons.A))
            {
                if (CanSelectMe)
                {
                    Value = !val;
                    if (ValueChangedEvent != null)
                        ValueChangedEvent.Invoke(ID);
                }
            }

            if (amount == 0)
                return;

            if (delay <= 0)
            {
                if (CanSelectMe)
                {
                    Value = !val;
                    if (ValueChangedEvent != null)
                        ValueChangedEvent.Invoke(ID);
                }

                delay = changeDelay;
            }
        }

    }
}
