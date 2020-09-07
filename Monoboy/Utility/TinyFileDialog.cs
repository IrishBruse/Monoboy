using System;
using System.Runtime.InteropServices;

namespace Monoboy.Utility
{
    internal class TinyFileDialog
    {
#pragma warning disable IDE1006 // Naming Styles
#if x86
        // cross platform UTF8
        [DllImport("tinyfiledialogsx86.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tinyfd_beep();

        [DllImport("tinyfiledialogsx86.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tinyfd_notifyPopup(string aTitle, string aMessage, string aIconType);

        [DllImport("tinyfiledialogsx86.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tinyfd_messageBox(string aTitle, string aMessage, string aDialogTyle, string aIconType, int aDefaultButton);

        [DllImport("tinyfiledialogsx86.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tinyfd_inputBox(string aTitle, string aMessage, string aDefaultInput);

        [DllImport("tinyfiledialogsx86.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tinyfd_saveFileDialog(string aTitle, string aDefaultPathAndFile, int aNumOfFilterPatterns, string[] aFilterPatterns, string aSingleFilterDescription);

        [DllImport("tinyfiledialogsx86.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tinyfd_openFileDialog(string aTitle, string aDefaultPathAndFile, int aNumOfFilterPatterns, string[] aFilterPatterns, string aSingleFilterDescription, int aAllowMultipleSelects);

        [DllImport("tinyfiledialogsx86.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tinyfd_selectFolderDialog(string aTitle, string aDefaultPathAndFile);

        [DllImport("tinyfiledialogsx86.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tinyfd_colorChooser(string aTitle, string aDefaultHexRGB, byte[] aDefaultRGB, byte[] aoResultRGB);      
#else
        // cross platform UTF8
        [DllImport("tinyfiledialogsx64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tinyfd_beep();

        [DllImport("tinyfiledialogsx64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tinyfd_notifyPopup(string aTitle, string aMessage, string aIconType);

        [DllImport("tinyfiledialogsx64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tinyfd_messageBox(string aTitle, string aMessage, string aDialogTyle, string aIconType, int aDefaultButton);

        [DllImport("tinyfiledialogsx64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tinyfd_inputBox(string aTitle, string aMessage, string aDefaultInput);

        [DllImport("tinyfiledialogsx64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tinyfd_saveFileDialog(string aTitle, string aDefaultPathAndFile, int aNumOfFilterPatterns, string[] aFilterPatterns, string aSingleFilterDescription);

        [DllImport("tinyfiledialogsx64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tinyfd_openFileDialog(string aTitle, string aDefaultPathAndFile, int aNumOfFilterPatterns, string[] aFilterPatterns, string aSingleFilterDescription, int aAllowMultipleSelects);

        [DllImport("tinyfiledialogsx64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tinyfd_selectFolderDialog(string aTitle, string aDefaultPathAndFile);

        [DllImport("tinyfiledialogsx64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tinyfd_colorChooser(string aTitle, string aDefaultHexRGB, byte[] aDefaultRGB, byte[] aoResultRGB);
#endif
#pragma warning restore IDE1006 // Naming Styles

        /// <summary>
        /// Makes a beep sound
        /// </summary>
        public static void Beep()
        {
            tinyfd_beep();
        }

        /// <summary>
        /// Creates a notfication with a message and icon
        /// </summary>
        /// <param name="title">Title Notifaction</param>
        /// <param name="message">Notification contents</param>
        /// <param name="iconType">info / warning / error</param>
        /// <returns></returns>
        public static int CreateNotification(string title, string message, string iconType)
        {
            return tinyfd_notifyPopup(title, message, iconType);
        }

        /// <summary>
        /// Create a message box popup
        /// </summary>
        /// <param name="title">Message popup</param>
        /// <param name="message">Message contents</param>
        /// <param name="dialogType">ok / okcancel / yesno / yesnocancel</param>
        /// <param name="iconType">info / warning / error</param>
        /// <param name="defaultButton"></param>
        /// <returns></returns>
        public static int CreateMessageBox(string title, string message, string dialogType, string iconType, int defaultButton)
        {
            return tinyfd_messageBox(title, message, dialogType, iconType, defaultButton);
        }

        public static string CreateInputBox(string title, string message, string defaultInput)
        {
            return StringFromAnsi(tinyfd_inputBox(title, message, defaultInput));
        }

        public static string SaveFileDialog(string title, string defaultPathAndFile, string[] filterPatterns, string singleFilterDescription)
        {
            return StringFromAnsi(tinyfd_saveFileDialog(title, defaultPathAndFile, filterPatterns.Length, filterPatterns, singleFilterDescription));
        }

        /// <summary>
        /// Open File
        /// </summary>
        /// <param name="title"></param>
        /// <param name="defaultPathAndFile"></param>
        /// <param name="filterPatterns"></param>
        /// <param name="singleFilterDescription"></param>
        /// <param name="allowMultipleSelects"></param>
        /// <returns>Absolute path of the file</returns>
        public static string OpenFileDialog(string title, string defaultPathAndFile, string[] filterPatterns, string singleFilterDescription, bool allowMultipleSelects)
        {
            return StringFromAnsi(tinyfd_openFileDialog(title, defaultPathAndFile, filterPatterns.Length, filterPatterns, singleFilterDescription, allowMultipleSelects ? 1 : 0));
        }

        public static string SelectFolderDialog(string title, string defaultPathAndFile)
        {
            return StringFromAnsi(tinyfd_selectFolderDialog(title, defaultPathAndFile));
        }

        public static string ColorChooser(string title, string defaultHexRGB, byte[] defaultRGB, byte[] resultRGB)
        {
            return StringFromAnsi(tinyfd_colorChooser(title, defaultHexRGB, defaultRGB, resultRGB));
        }

        public static string StringFromAnsi(IntPtr ptr)
        {
            return Marshal.PtrToStringAnsi(ptr);
        }
    }
}
