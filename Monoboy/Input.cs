namespace Monoboy.Core
{
    public class Input
    {
        byte buttonState;
        byte dirpadState;

        Bus bus;

        public Input(Bus bus)
        {
            this.bus = bus;
        }

        public void SetButton(Button button, bool state)
        {
            byte key = (byte)button;

            if(key > 8)
            {
                buttonState = (byte)(key >> 4);
            }
            else
            {
                dirpadState = key;
            }
        }
    }
}