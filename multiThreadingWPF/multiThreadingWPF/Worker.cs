using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Excel = Microsoft.Office.Interop.Excel;    
using System.Data;                        
using System.Windows;      


namespace multiThreadingWPF
{

    public class Worker
    {
        private bool _cancelled = false;
        private DataClassString3[] list;
        private DataClassString2[] blackList;
        private List<DataClassString4> listOfResultsNotUniq;
        private List<DataClassString3> listOfResultsNotValid;
        private List<DataClassString4> listOfResultsInBlackList;
        private int lastColumn;
        private int lastRow;


        public void Cancel()
        {
            _cancelled = true;
        }


        public void Work ()
        {
            for (int i=0; i<100; i++)
            {
                if (_cancelled)
                    break;
                Thread.Sleep(50);
                ProcessChanged(i);
            }
            WorkCompleted(_cancelled);
        }


        public void ReadTheFile(object fileName)
        {
            ProcessChanged(0);
            StageChanged("Loading information from selected file");

            string fileNameString;
            fileNameString = ((DataClassString2)fileName).N1;
            Excel.Application ObjWorkExcel = new Excel.Application(); 
            Excel.Workbook ObjWorkBook = ObjWorkExcel.Workbooks.Open(fileNameString);
            Excel.Worksheet ObjWorkSheet = (Excel.Worksheet)ObjWorkBook.Sheets[1]; 

            var lastCell = ObjWorkSheet.Cells.SpecialCells(Excel.XlCellType.xlCellTypeLastCell);
            lastColumn = (int)lastCell.Column;
            lastRow = (int)lastCell.Row;

            for (int i = 1; i <= lastColumn; i++)  
                ObjWorkSheet.Columns[i].AutoFit();

            list= new DataClassString3[lastRow-1];
            for (int i = 0; i < lastRow-1; i++) 
                {
                    list[i] = new DataClassString3();
                    list[i].N1 = ObjWorkSheet.Cells[i + 2, 1].Text.ToString();
                    list[i].N1 = list[i].N1.Trim();  
                    list[i].N2 = ObjWorkSheet.Cells[i + 2, 2].Text.ToString();
                    list[i].N2 = list[i].N2.Trim();  
                    list[i].N3 = ObjWorkSheet.Cells[i + 2, 3].Text.ToString();
                    list[i].N3 = list[i].N3.Trim(); 
                }

            ObjWorkBook.Close(false, Type.Missing, Type.Missing); 
            ObjWorkExcel.Quit(); 
            GC.Collect(); 

            ProcessChanged(10);
        }


        public void ReadTheBlackList(object fileName)
        {
            StageChanged("Loading information from the blacklist");

            string blistNameString;
            blistNameString = ((DataClassString2)fileName).N2;
            Excel.Application ObjWorkExcel = new Excel.Application(); 
            Excel.Workbook ObjWorkBook = ObjWorkExcel.Workbooks.Open(blistNameString);
            Excel.Worksheet ObjWorkSheet = (Excel.Worksheet)ObjWorkBook.Sheets[1];

            var lastCell = ObjWorkSheet.Cells.SpecialCells(Excel.XlCellType.xlCellTypeLastCell);
            int lastColumnBL = (int)lastCell.Column;
            int lastRowBL = (int)lastCell.Row;

            for (int i = 1; i <= lastColumnBL; i++)    
                ObjWorkSheet.Columns[i].AutoFit();

            blackList = new DataClassString2[lastRowBL-1];
            for (int i = 0; i < lastRowBL-1; i++) 
            {
                blackList[i] = new DataClassString2();
                blackList[i].N1 = ObjWorkSheet.Cells[i + 2, 1].Text.ToString();
                blackList[i].N1 = blackList[i].N1.Trim();  
                blackList[i].N2 = ObjWorkSheet.Cells[i + 2, 2].Text.ToString();
                blackList[i].N2 = blackList[i].N2.Trim();  
            }

            ObjWorkBook.Close(false, Type.Missing, Type.Missing); 
            ObjWorkExcel.Quit(); 
            GC.Collect(); 

            ProcessChanged(20);
        }


        public void WriteResultsIntoFile(object fileName)
        {
            if (_cancelled == false)
            {
                StageChanged("Writing results into new file");
                Excel.Application ObjExcelResults = new Excel.Application();
                Excel.Workbook ObjWorkBookResults;
                Excel.Worksheet ObjWorkSheetResults;
                ObjExcelResults.SheetsInNewWorkbook = 3;    
                ObjWorkBookResults = ObjExcelResults.Workbooks.Add(System.Reflection.Missing.Value);
                ObjExcelResults.Sheets[1].Name = "notUnique";
                ObjExcelResults.Sheets[2].Name = "notValid";      
                ObjExcelResults.Sheets[3].Name = "inBlackList";

                // sheet NotUnique

                ObjWorkSheetResults = (Excel.Worksheet)ObjWorkBookResults.Sheets[1];
                Excel.Range   excelcells;                     
                excelcells = ObjWorkSheetResults.get_Range("A1", "D1");
                excelcells.Borders.ColorIndex = 1;                                  // set the colour of the line
                excelcells.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;      // set the style of the line
                excelcells.Borders.Weight = Excel.XlBorderWeight.xlMedium;           // set the width of the line
                excelcells.Interior.ColorIndex = 15;                                 // set the colour of cells
                excelcells.Interior.PatternColorIndex = Excel.Constants.xlAutomatic;

                ObjWorkSheetResults.Cells[1, 1] = "NAME";
                ObjWorkSheetResults.Cells[1, 2] = "SURNAME";
                ObjWorkSheetResults.Cells[1, 3] = "TAXID";
                ObjWorkSheetResults.Cells[1, 4] = "QUANTITY";

                for (int i = 0; i < listOfResultsNotUniq.Count; i++) 
                {
                    ObjWorkSheetResults.Cells[i + 2, 1] = listOfResultsNotUniq[i].N1;
                    ObjWorkSheetResults.Cells[i + 2, 2] = listOfResultsNotUniq[i].N2;
                    ObjWorkSheetResults.Cells[i + 2, 3] = listOfResultsNotUniq[i].N3;
                    ObjWorkSheetResults.Cells[i + 2, 4] = listOfResultsNotUniq[i].N4;
                }
                for (int i = 1; i <=4; i++)     
                    ObjWorkSheetResults.Columns[i].AutoFit();

                // sheet NotValid

                ObjWorkSheetResults = (Excel.Worksheet)ObjWorkBookResults.Sheets[2];
                excelcells = ObjWorkSheetResults.get_Range("A1", "C1");
                excelcells.Borders.ColorIndex = 1;                                  // set the colour of the line
                excelcells.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;      // set the style of the line
                excelcells.Borders.Weight = Excel.XlBorderWeight.xlMedium;           // set the width of the line
                excelcells.Interior.ColorIndex = 15;                                 // set the colour of cells
                excelcells.Interior.PatternColorIndex = Excel.Constants.xlAutomatic;

                ObjWorkSheetResults.Cells[1, 1] = "NAME";
                ObjWorkSheetResults.Cells[1, 2] = "SURNAME";
                ObjWorkSheetResults.Cells[1, 3] = "TAXID";

                for (int i = 0; i < listOfResultsNotValid.Count; i++) 
                {
                    ObjWorkSheetResults.Cells[i + 2, 1] = listOfResultsNotValid[i].N1;
                    ObjWorkSheetResults.Cells[i + 2, 2] = listOfResultsNotValid[i].N2;
                    ObjWorkSheetResults.Cells[i + 2, 3] = listOfResultsNotValid[i].N3;
                }
                for (int i = 1; i <= 3; i++)      
                    ObjWorkSheetResults.Columns[i].AutoFit();

                // sheet InBlackList

                ObjWorkSheetResults = (Excel.Worksheet)ObjWorkBookResults.Sheets[3];
                excelcells = ObjWorkSheetResults.get_Range("A1", "D1");
                excelcells.Borders.ColorIndex = 1;                                  // set the colour of the line
                excelcells.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;      // set the style of the line
                excelcells.Borders.Weight = Excel.XlBorderWeight.xlMedium;           // set the width of the line
                excelcells.Interior.ColorIndex = 15;                                 // set the colour of cells
                excelcells.Interior.PatternColorIndex = Excel.Constants.xlAutomatic;

                ObjWorkSheetResults.Cells[1, 1] = "NAME";
                ObjWorkSheetResults.Cells[1, 2] = "SURNAME";
                ObjWorkSheetResults.Cells[1, 3] = "TAXID";
                ObjWorkSheetResults.Cells[1, 4] = "REASON";

                for (int i = 0; i < listOfResultsInBlackList.Count; i++) 
                {
                    ObjWorkSheetResults.Cells[i + 2, 1] = listOfResultsInBlackList[i].N1;
                    ObjWorkSheetResults.Cells[i + 2, 2] = listOfResultsInBlackList[i].N2;
                    ObjWorkSheetResults.Cells[i + 2, 3] = listOfResultsInBlackList[i].N3;
                    ObjWorkSheetResults.Cells[i + 2, 4] = listOfResultsInBlackList[i].N4;
                }
                for (int i = 1; i <= 4; i++)    
                    ObjWorkSheetResults.Columns[i].AutoFit();

                //---------------------------------------------

                ObjExcelResults.Visible = true;
                ObjExcelResults.Quit(); 
                GC.Collect();
            }

            ProcessChanged(100);
            WorkCompleted(_cancelled);
            
        }


        public void SearchTaxIdInBlackList(object fileName)
        {

            if (_cancelled == false)
            {
                StageChanged("Checking TAXIDs in the blacklist");

                var queryBlackL = from tttaxid in list
                              from bbblack in blackList
                              where tttaxid.N3 == bbblack.N1
                              select new
                              {
                                  N1 = tttaxid.N1,
                                  N2 = tttaxid.N2,
                                  N3 = tttaxid.N3,
                                  N4 = bbblack.N2
                              };

                listOfResultsInBlackList = new List<DataClassString4>();
                foreach (var st in queryBlackL)
                {
                    DataClassString4 resultExample = new DataClassString4();
                    resultExample.N1 = st.N1;
                    resultExample.N2 = st.N2;
                    resultExample.N3 = st.N3.ToString();
                    resultExample.N4 = st.N4;
                    listOfResultsInBlackList.Add(resultExample);
                }

                ListBoxInBlackListEvent(listOfResultsInBlackList);
            }
            ProcessChanged(80);
        }


        public void SearchNotUniqTaxId(object fileName)
        {
            if (_cancelled == false)
            {
                StageChanged("Searching for not uniq TAXIDs");

                var queryNotUniq = from tttaxid in list
                               group tttaxid by tttaxid.N3 into g
                               select new
                               {
                                   N1 = g.Key,
                                   N2 = g.Count(),
                                   N3 = ""
                               } into uuu
                               where uuu.N2 > 1
                               select uuu;

                var queryNotUniq2 = from mmm1 in queryNotUniq
                                from mmm2 in list
                                where mmm1.N1 == mmm2.N3
                                orderby mmm1.N2 descending
                                orderby mmm1.N1
                                select new
                                {
                                    N1 = mmm2.N1,
                                    N2 = mmm2.N2,
                                    N3 = mmm2.N3,
                                    N4 = mmm1.N2
                                };

                listOfResultsNotUniq = new List<DataClassString4>();
                foreach (var st in queryNotUniq2)
                {
                    DataClassString4 resultExample = new DataClassString4();
                    resultExample.N1 = st.N1;
                    resultExample.N2 = st.N2;
                    resultExample.N3 = st.N3;
                    resultExample.N4 = st.N4.ToString();
                    listOfResultsNotUniq.Add(resultExample);
                }

                ListBoxNotUniqEvent(listOfResultsNotUniq);
            }
            ProcessChanged(60);
        }


        public void ValidationTaxId(object fileName)
        {
            if (_cancelled == false)
            {
                StageChanged("Validation TAXIDs");
                listOfResultsNotValid = new List<DataClassString3>();
                long taxidForValidation;
                double res;
                double validationNumber;
                double validationNumberCalculated;
                
                for (int i = 0; i < lastRow-1; i++)
                {

                    try { taxidForValidation = Convert.ToInt64(list[i].N3); }
                    catch { taxidForValidation = -100; };
                    res = -Math.Truncate(taxidForValidation / 1000000000.0) + 5 * (Math.Truncate(taxidForValidation / 100000000.0) % 10) + 7 * (Math.Truncate(taxidForValidation / 10000000.0) % 10)
                          + 9 * (Math.Truncate(taxidForValidation / 1000000.0) % 10) + 4 * (Math.Truncate(taxidForValidation / 100000.0) % 10) + 6 * (Math.Truncate(taxidForValidation / 10000.0) % 10)
                          + 10 * (Math.Truncate(taxidForValidation / 1000.0) % 10) + 5 * (Math.Truncate(taxidForValidation / 100.0) % 10) + 7 * (Math.Truncate(taxidForValidation / 10.0) % 10);
                    //  http://zliypes.com.ua/blog/2007/10/29/ukrainian-pti/   --   algorithm of validation
                    validationNumber = taxidForValidation % 10;
                    validationNumberCalculated = res - 11 * Math.Truncate(res / 11);

                    if (taxidForValidation < 0 || Math.Abs(validationNumber - validationNumberCalculated) > 0.01)
                        listOfResultsNotValid.Add(list[i]);
                }
                ListBoxNotValidEvent(listOfResultsNotValid);
            }
            ProcessChanged(40);
        }


        public event Action<int> ProcessChanged;
        public event Action<string> StageChanged;
        public event Action<bool> WorkCompleted;
        public event Action<List<DataClassString4>> ListBoxNotUniqEvent;
        public event Action<List<DataClassString3>> ListBoxNotValidEvent;
        public event Action<List<DataClassString4>> ListBoxInBlackListEvent;

    }
}


