using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace NppPluginNET
{
    partial class PluginBase
    {
        #region " Fields "
        internal const string PluginName = "GenerateGuidUtils";
        static string iniFilePath = null;
        static string sectionName = "Insert Extension";
        static string keyName = "doCloseTag";
        static bool doCloseTag = false;
        static string sessionFilePath = @"C:\text.session";
        static frmGoToLine frmGoToLine = null;
        static internal int idFrmGotToLine = -1;
        static Bitmap tbBmp = Properties.Resources.star;
        static Bitmap tbBmp_tbTab = Properties.Resources.star_bmp;
        static Icon tbIcon = null;
        #endregion

        #region " Startup/CleanUp "
        static internal void CommandMenuInit()
        {
            // Initialization of your plugin commands
            // You should fill your plugins commands here
 
        	//
	        // Firstly we get the parameters from your plugin config file (if any)
	        //

	        // get path of plugin configuration
            StringBuilder sbIniFilePath = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(nppData._nppHandle, NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbIniFilePath);
            iniFilePath = sbIniFilePath.ToString();

	        // if config path doesn't exist, we create it
            if (!Directory.Exists(iniFilePath))
	        {
                Directory.CreateDirectory(iniFilePath);
	        }

	        // make your plugin config file full file path name
            iniFilePath = Path.Combine(iniFilePath, PluginName + ".ini");

	        // get the parameter value from plugin config
	        doCloseTag = (Win32.GetPrivateProfileInt(sectionName, keyName, 0, iniFilePath) != 0);

            // with function :
            // SetCommand(int index,                            // zero based number to indicate the order of command
            //            string commandName,                   // the command name that you want to see in plugin menu
            //            NppFuncItemDelegate functionPointer,  // the symbol of function (function pointer) associated with this command. The body should be defined below. See Step 4.
            //            ShortcutKey *shortcut,                // optional. Define a shortcut to trigger this command
            //            bool check0nInit                      // optional. Make this menu item be checked visually
            //            );
            SetCommand(0, "Generate GUID Upper", UpperGuid);
            SetCommand(1, "Generate GUID ANSI", AnsiGuid);
            SetCommand(2, "---", null);
            SetCommand(3, "Convert Guid to UPPER", ToUpperGuid);
            SetCommand(4, "Convert Guid to Ansi", ToAnsiGuid);

            

            // Shortcut :
            // Following makes the command bind to the shortcut Alt-F
            //SetCommand(4, "Current Full Path", insertCurrentFullPath, new ShortcutKey(false, true, false, Keys.F));
            //SetCommand(5, "Current File Name", insertCurrentFileName);
            //SetCommand(6, "Current Directory", insertCurrentDirectory);
            //SetCommand(7, "Date && Time - short format", insertShortDateTime);
            //SetCommand(8, "Date && Time - long format", insertLongDateTime);

            //SetCommand(9, "Close HTML/XML tag automatically", checkInsertHtmlCloseTag, new ShortcutKey(false, true, false, Keys.Q), doCloseTag);

            //SetCommand(10, "---", null);

            //SetCommand(11, "Get File Names Demo", getFileNamesDemo);
            //SetCommand(12, "Get Session File Names Demo", getSessionFileNamesDemo);
            //SetCommand(13, "Save Current Session Demo", saveCurrentSessionDemo);

            //SetCommand(14, "---", null);

            //SetCommand(15, "Dockable Dialog Demo", DockableDlgDemo); 
            
            idFrmGotToLine = 4;
        }
        static internal void SetToolBarIcon()
        {
            toolbarIcons tbIcons = new toolbarIcons();
            tbIcons.hToolbarBmp = tbBmp.GetHbitmap();
            IntPtr pTbIcons = Marshal.AllocHGlobal(Marshal.SizeOf(tbIcons));
            Marshal.StructureToPtr(tbIcons, pTbIcons, false);
            Win32.SendMessage(nppData._nppHandle, NppMsg.NPPM_ADDTOOLBARICON, _funcItems.Items[idFrmGotToLine]._cmdID, pTbIcons);
            Marshal.FreeHGlobal(pTbIcons);
        }
        static internal void PluginCleanUp()
        {
	        Win32.WritePrivateProfileString(sectionName, keyName, doCloseTag ? "1" : "0", iniFilePath);
        }
        #endregion

        public static string GetDocumentText(IntPtr curScintilla)
        {
            IntPtr hCurrScintilla = PluginBase.GetCurrentScintilla();
            int textLen = (int)Win32.SendMessage(hCurrScintilla, SciMsg.SCI_GETSELTEXT, 0, 0);
            IntPtr ptrText = Marshal.AllocHGlobal(textLen);
            Win32.SendMessage(hCurrScintilla, SciMsg.SCI_GETSELTEXT, 0, ptrText);
            // get it as string but truncated..
            string s = Marshal.PtrToStringAnsi(ptrText);
            // or everything.. well as byte array
            byte[] b = new byte[textLen];
            Marshal.Copy(ptrText, b, 0, textLen);

            Marshal.FreeHGlobal(ptrText);
            return s;

            //int start = (int)Win32.SendMessage(curScintilla, SciMsg.SCI_GETSELECTIONNEND, 0, 0);
            //int end = (int)Win32.SendMessage(curScintilla, SciMsg.SCI_GETSELECTIONNEND, 0, 0);
            //int textLen2 = end - start;
            //StringBuilder sb = new StringBuilder();
            //Win32.SendMessage(curScintilla, SciMsg.SCI_GETSELTEXT, 0, sb);
            //return sb.ToString();
        }

        #region " Menu functions "
        static void UpperGuid()
        {
            var guid = Guid.NewGuid().ToString("N").ToUpper();

            Win32.SendMessage(GetCurrentScintilla(), SciMsg.SCI_REPLACESEL, 0, guid);
        }
        static void ToUpperGuid()
        {
            try
            {
                var txt = GetDocumentText(GetCurrentScintilla());

                var guid = new Guid(txt).ToString("N").ToUpper();
                
                Win32.SendMessage(GetCurrentScintilla(), SciMsg.SCI_REPLACESEL, 0, guid);
            }
            catch (Exception ex)
            {
                Win32.SendMessage(GetCurrentScintilla(), SciMsg.SCI_REPLACESEL, 0, ex.ToString());
            }
        }

        static void AnsiGuid()
        {
            var guid = Guid.NewGuid().ToString();

            Win32.SendMessage(GetCurrentScintilla(), SciMsg.SCI_REPLACESEL, 0, guid);
        }
        static void ToAnsiGuid()
        {
            try
            {
                var txt = GetDocumentText(GetCurrentScintilla());

                var guid = new Guid(txt).ToString();

                Win32.SendMessage(GetCurrentScintilla(), SciMsg.SCI_REPLACESEL, 0, guid);
            }
            catch
            {
            }
        }


        static void callbackHelloFX()
        {
            IntPtr curScintilla = GetCurrentScintilla();
            int currentZoomLevel = (int)Win32.SendMessage(curScintilla, SciMsg.SCI_GETZOOM, 0, 0);
            int i = currentZoomLevel;
            for (int j = 0 ; j < 4 ; j++)
            {	
	            for ( ; i >= -10; i--)
	            {
		            Win32.SendMessage(curScintilla, SciMsg.SCI_SETZOOM, i, 0);
                    Thread.Sleep(30);
	            }
                Thread.Sleep(100);
	            for ( ; i <= 20 ; i++)
	            {
		            Thread.Sleep(30);
		            Win32.SendMessage(curScintilla, SciMsg.SCI_SETZOOM, i, 0);
	            }
                Thread.Sleep(100);
            }
            for ( ; i >= currentZoomLevel ; i--)
            {
                Thread.Sleep(30);
                Win32.SendMessage(curScintilla, SciMsg.SCI_SETZOOM, i, 0);
            }
        }
        static void WhatIsNpp()
        {
            string text2display = "Notepad++ is a free (as in \"free speech\" and also as in \"free beer\") " +
                "source code editor and Notepad replacement that supports several languages.\n" +
		        "Running in the MS Windows environment, its use is governed by GPL License.\n\n" +
                "Based on a powerful editing component Scintilla, Notepad++ is written in C++ and " +
                "uses pure Win32 API and STL which ensures a higher execution speed and smaller program size.\n" +
                "By optimizing as many routines as possible without losing user friendliness, Notepad++ is trying " +
                "to reduce the world carbon dioxide emissions. When using less CPU power, the PC can throttle down " +
                "and reduce power consumption, resulting in a greener environment.";
            new Thread(new ParameterizedThreadStart(callbackWhatIsNpp)).Start(text2display);
        }
        static void callbackWhatIsNpp(object data)
        {
            string text2display = (string)data;
            // Open a new document
            Win32.SendMessage(nppData._nppHandle, NppMsg.NPPM_MENUCOMMAND, 0, NppMenuCmd.IDM_FILE_NEW);

            // Get the current scintilla
            IntPtr curScintilla = GetCurrentScintilla();

            Random srand = new Random(DateTime.Now.Millisecond);
            int rangeMin = 0;
            int rangeMax = 250;
            for (int i = 0; i < text2display.Length; i++)
            {
                StringBuilder charToShow = new StringBuilder(text2display[i].ToString());

                int ranNum = srand.Next(rangeMin, rangeMax);
                Thread.Sleep(ranNum + 30);

                Win32.SendMessage(curScintilla, SciMsg.SCI_APPENDTEXT, 1, charToShow);
                Win32.SendMessage(curScintilla, SciMsg.SCI_GOTOPOS, (int)Win32.SendMessage(curScintilla, SciMsg.SCI_GETLENGTH, 0, 0), 0);
            }
        }

        static void insertCurrentFullPath()
        {
            insertCurrentPath(NppMsg.FULL_CURRENT_PATH);
        }
        static void insertCurrentFileName()
        {
            insertCurrentPath(NppMsg.FILE_NAME);
        }
        static void insertCurrentDirectory()
        {
            insertCurrentPath(NppMsg.CURRENT_DIRECTORY);
        }
        static void insertCurrentPath(NppMsg which)
        {
	        NppMsg msg = NppMsg.NPPM_GETFULLCURRENTPATH;
	        if (which == NppMsg.FILE_NAME)
		        msg = NppMsg.NPPM_GETFILENAME;
	        else if (which == NppMsg.CURRENT_DIRECTORY)
                msg = NppMsg.NPPM_GETCURRENTDIRECTORY;

	        StringBuilder path = new StringBuilder(Win32.MAX_PATH);
	        Win32.SendMessage(nppData._nppHandle, msg, 0, path);

            Win32.SendMessage(GetCurrentScintilla(), SciMsg.SCI_REPLACESEL, 0, path);
        }

        static void insertShortDateTime()
        {
            insertDateTime(false);
        }
        static void insertLongDateTime()
        {
            insertDateTime(true);
        }
        static void insertDateTime(bool longFormat)
        {
            string dateTime = string.Format("{0} {1}", 
                DateTime.Now.ToShortTimeString(),
                longFormat ? DateTime.Now.ToLongDateString() : DateTime.Now.ToShortDateString());
            Win32.SendMessage(GetCurrentScintilla(), SciMsg.SCI_REPLACESEL, 0, dateTime);
        }

        static void checkInsertHtmlCloseTag()
        {
            doCloseTag = !doCloseTag;

            int i = Win32.CheckMenuItem(Win32.GetMenu(nppData._nppHandle), _funcItems.Items[9]._cmdID,
                Win32.MF_BYCOMMAND | (doCloseTag ? Win32.MF_CHECKED : Win32.MF_UNCHECKED));
        }
        static internal void doInsertHtmlCloseTag(char newChar)
        {
            LangType docType = LangType.L_TEXT;
            Win32.SendMessage(nppData._nppHandle, NppMsg.NPPM_GETCURRENTLANGTYPE, 0, ref docType);
            bool isDocTypeHTML = (docType == LangType.L_HTML || docType == LangType.L_XML || docType == LangType.L_PHP);
            if (doCloseTag && isDocTypeHTML)
            {
                if (newChar == '>')
                {
                    int bufCapacity = 512;
                    IntPtr hCurrentEditView = GetCurrentScintilla();
                    int currentPos = (int)Win32.SendMessage(hCurrentEditView, SciMsg.SCI_GETCURRENTPOS, 0, 0);
                    int beginPos = currentPos - (bufCapacity - 1);
                    int startPos = (beginPos > 0) ? beginPos : 0;
                    int size = currentPos - startPos;

                    if (size >= 3)
                    {
                        using (Sci_TextRange tr = new Sci_TextRange(startPos, currentPos, bufCapacity))
                        {
                            Win32.SendMessage(hCurrentEditView, SciMsg.SCI_GETTEXTRANGE, 0, tr.NativePointer);
                            string buf = tr.lpstrText;

                            if (buf[size - 2] != '/')
                            {
                                StringBuilder insertString = new StringBuilder("</");

                                int pCur = size - 2;
                                for (; (pCur > 0) && (buf[pCur] != '<') && (buf[pCur] != '>'); )
                                    pCur--;

                                if (buf[pCur] == '<')
                                {
                                    pCur++;

                                    Regex regex = new Regex(@"[\._\-:\w]");
                                    while (regex.IsMatch(buf[pCur].ToString()))
                                    {
                                        insertString.Append(buf[pCur]);
                                        pCur++;
                                    }
                                    insertString.Append('>');

                                    if (insertString.Length > 3)
                                    {
                                        Win32.SendMessage(hCurrentEditView, SciMsg.SCI_BEGINUNDOACTION, 0, 0);
                                        Win32.SendMessage(hCurrentEditView, SciMsg.SCI_REPLACESEL, 0, insertString);
                                        Win32.SendMessage(hCurrentEditView, SciMsg.SCI_SETSEL, currentPos, currentPos);
                                        Win32.SendMessage(hCurrentEditView, SciMsg.SCI_ENDUNDOACTION, 0, 0);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        static void getFileNamesDemo()
        {
            int nbFile = (int)Win32.SendMessage(nppData._nppHandle, NppMsg.NPPM_GETNBOPENFILES, 0, 0);
            MessageBox.Show(nbFile.ToString(), "Number of opened files:");

            using (ClikeStringArray cStrArray = new ClikeStringArray(nbFile, Win32.MAX_PATH))
            {
                if (Win32.SendMessage(nppData._nppHandle, NppMsg.NPPM_GETOPENFILENAMES, cStrArray.NativePointer, nbFile) != IntPtr.Zero)
                    foreach (string file in cStrArray.ManagedStringsUnicode) MessageBox.Show(file);
            }
        }
        static void getSessionFileNamesDemo()
        {
            int nbFile = (int)Win32.SendMessage(nppData._nppHandle, NppMsg.NPPM_GETNBSESSIONFILES, 0, sessionFilePath);

        	if (nbFile < 1)
	        {
		        MessageBox.Show("Please modify \"sessionFilePath\" in \"Demo.cs\" in order to point to a valid session file", "Error");
		        return;
	        }
            MessageBox.Show(nbFile.ToString(), "Number of session files:");

            using (ClikeStringArray cStrArray = new ClikeStringArray(nbFile, Win32.MAX_PATH))
            {
                if (Win32.SendMessage(nppData._nppHandle, NppMsg.NPPM_GETSESSIONFILES, cStrArray.NativePointer, sessionFilePath) != IntPtr.Zero)
                    foreach (string file in cStrArray.ManagedStringsUnicode) MessageBox.Show(file);
            }
        }
        static void saveCurrentSessionDemo()
        {
            string sessionPath = Marshal.PtrToStringUni(Win32.SendMessage(nppData._nppHandle, NppMsg.NPPM_SAVECURRENTSESSION, 0, sessionFilePath));
	        if (!string.IsNullOrEmpty(sessionPath))
		        MessageBox.Show(sessionPath, "Saved Session File :", MessageBoxButtons.OK);
        }

        static void DockableDlgDemo()
        {
            // Dockable Dialog Demo
            // 
            // This demonstration shows you how to do a dockable dialog.
            // You can create your own non dockable dialog - in this case you don't nedd this demonstration.
            if (frmGoToLine == null)
            {
                frmGoToLine = new frmGoToLine();

                using (Bitmap newBmp = new Bitmap(16, 16))
                {
					Graphics g = Graphics.FromImage(newBmp);
					ColorMap[] colorMap = new ColorMap[1];
					colorMap[0] = new ColorMap();
					colorMap[0].OldColor = Color.Fuchsia;
					colorMap[0].NewColor = Color.FromKnownColor(KnownColor.ButtonFace);
					ImageAttributes attr = new ImageAttributes();
					attr.SetRemapTable(colorMap);
					g.DrawImage(tbBmp_tbTab, new Rectangle(0, 0, 16, 16), 0, 0, 16, 16, GraphicsUnit.Pixel, attr);
					tbIcon = Icon.FromHandle(newBmp.GetHicon());
                }
                
                NppTbData _nppTbData = new NppTbData();
                _nppTbData.hClient = frmGoToLine.Handle;
                _nppTbData.pszName = "Go To Line #";
                // the dlgDlg should be the index of funcItem where the current function pointer is in
                // this case is 15.. so the initial value of funcItem[15]._cmdID - not the updated internal one !
                _nppTbData.dlgID = idFrmGotToLine;
                // define the default docking behaviour
                _nppTbData.uMask = NppTbMsg.DWS_DF_CONT_RIGHT | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR;
                _nppTbData.hIconTab = (uint)tbIcon.Handle;
                _nppTbData.pszModuleName = PluginName;
                IntPtr _ptrNppTbData = Marshal.AllocHGlobal(Marshal.SizeOf(_nppTbData));
                Marshal.StructureToPtr(_nppTbData, _ptrNppTbData, false);

                Win32.SendMessage(nppData._nppHandle, NppMsg.NPPM_DMMREGASDCKDLG, 0, _ptrNppTbData);
                // Following message will toogle both menu item state and toolbar button
                Win32.SendMessage(nppData._nppHandle, NppMsg.NPPM_SETMENUITEMCHECK, _funcItems.Items[idFrmGotToLine]._cmdID, 1);
            }
            else
            {
            	if (!frmGoToLine.Visible)
            	{
	                Win32.SendMessage(nppData._nppHandle, NppMsg.NPPM_DMMSHOW, 0, frmGoToLine.Handle);
	                Win32.SendMessage(nppData._nppHandle, NppMsg.NPPM_SETMENUITEMCHECK, _funcItems.Items[idFrmGotToLine]._cmdID, 1);
            	}
            	else
            	{
	                Win32.SendMessage(nppData._nppHandle, NppMsg.NPPM_DMMHIDE, 0, frmGoToLine.Handle);
	                Win32.SendMessage(nppData._nppHandle, NppMsg.NPPM_SETMENUITEMCHECK, _funcItems.Items[idFrmGotToLine]._cmdID, 0);
            	}
            }
            frmGoToLine.textBox1.Focus();
        }
        #endregion
    }
}   
