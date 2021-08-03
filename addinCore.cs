//*********************************************************************
//getLink
//Copyright (c) 2021 Nikolay Korobiy
//Product URL: https://github.com/korbnic/getLink
//License: https://github.com/korbnic/getLink/blob/main/LICENSE
//*********************************************************************
using System;
using System.Runtime.InteropServices;
using EPDM.Interop.epdm;

namespace getLink
{
    [ComVisible(true)]
    [Guid("36F7CB73-2FF3-4BAE-A27E-4C5A509779EC")]
    public class addinCore : IEdmAddIn5
    {
        private const string bc_window = "_getLink_"; //flag used to track the desired button click in DataCard
        [STAThread]
        public void GetAddInInfo(ref EdmAddInInfo poInfo, IEdmVault5 poVault, IEdmCmdMgr5 poCmdMgr)
        {
            try
            {
                //Information to display in the add-in's Properties dialog box 
                poInfo.mbsAddInName = "getLink";
                poInfo.mbsCompany = "korbman";
                poInfo.mbsDescription = "Allows the user to get different types of links to the selected file. Autor: korobokolas@gmail.com";
                poInfo.mlAddInVersion = 1010000;
                //Minimum required version of SOLIDWORKS PDM Professional
                poInfo.mlRequiredVersionMajor = 17;
                poInfo.mlRequiredVersionMinor = 5;

                poCmdMgr.AddCmd(1000, "get Link...", (int)EdmMenuFlags.EdmMenu_MustHaveSelection + (int)EdmMenuFlags.EdmMenu_OnlyFiles + (int)EdmMenuFlags.EdmMenu_OnlySingleSelection); //Вызов из контекстного меню. "get Link..." в контекстном меню отображается только при выборе ОДНОГО ФАЙЛА
                poCmdMgr.AddHook(EdmCmdType.EdmCmd_CardButton); //Реагируем по нажатию кнопки в DataCard
            }
            catch (COMException ex)
            {
                string errorName, errorDesc;
                poVault.GetErrorString(ex.ErrorCode, out errorName, out errorDesc);
                poVault.MsgBox(0, errorDesc, EdmMBoxType.EdmMbt_OKOnly, errorName);
            }
        }
        public void OnCmd(ref EdmCmd poCmd, ref EdmCmdData[] ppoData)
        {
            IEdmVault5 tmpVault = poCmd.mpoVault as IEdmVault5; //link to the current vault
            try
            {
                switch (poCmd.meCmdType)
                {
                    case EdmCmdType.EdmCmd_CardButton:
                    case EdmCmdType.EdmCmd_Menu:
                        if (poCmd.mbsComment == bc_window | poCmd.mlCmdID == 1000)
                        {
                            IEdmFile5 dcFile = tmpVault.GetObject(EdmObjectType.EdmObject_File, ppoData[0].mlObjectID1) as IEdmFile5;
                            if (dcFile != null)
                            {
                                IEdmPos5 folder_POS = dcFile.GetFirstFolderPosition();
                                IEdmFolder5 dcFolder = dcFile.GetNextFolder(folder_POS);
                                string fPath = dcFolder.LocalPath;
                                addinWindow f = new addinWindow(tmpVault.Name, dcFolder.ID, dcFile.ID, dcFile.Name, dcFolder.LocalPath);
                                f.ShowDialog();
                            }
                        }
                        break;

                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (COMException ex)
            {
                string errorName, errorDesc;
                tmpVault.GetErrorString(ex.ErrorCode, out errorName, out errorDesc);
                tmpVault.MsgBox(0, errorDesc, EdmMBoxType.EdmMbt_OKOnly, errorName);
            }
        }
    }
}
