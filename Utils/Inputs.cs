namespace Custom.Inputs
{
    public static class Get
    {
        public static bool rightGrab => ControllerInputPoller.instance.rightGrab;
        public static bool leftGrab => ControllerInputPoller.instance.leftGrab;
        public static bool aButton => ControllerInputPoller.instance.rightControllerPrimaryButton;
        public static bool bButton => ControllerInputPoller.instance.rightControllerSecondaryButton;
        public static bool yButton => ControllerInputPoller.instance.leftControllerSecondaryButton;
        public static bool xButton => ControllerInputPoller.instance.leftControllerPrimaryButton;
        public static bool rTrigger => ControllerInputPoller.instance.rightControllerTriggerButton;
        public static bool lTrigger => ControllerInputPoller.instance.leftControllerTriggerButton;
        public static bool rGripFloat => ControllerInputPoller.instance.rightControllerGripFloat > 0f;
        public static bool lGripFloat => ControllerInputPoller.instance.leftControllerGripFloat > 0f;
        public static bool rIndexFloat => ControllerInputPoller.instance.rightControllerIndexFloat > 0f;
        public static bool lIndexFloat => ControllerInputPoller.instance.leftControllerIndexFloat > 0f;
    }
}
