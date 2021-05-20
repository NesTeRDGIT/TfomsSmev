using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ExcelManager;
using Microsoft.Win32;
using SMEV.WCFContract;

namespace AddapterSMEVClient.Class
{
    public interface IReportExport
    {
        void Export(List<ReportRow> Report, DateTime Date_b, DateTime Date_e);
    }

    public class ReportExport : IReportExport
    {
        SaveFileDialog sfd = new SaveFileDialog {Filter = "*.xlsx|*.xlsx"};

        public void Export(List<ReportRow> Report, DateTime Date_b, DateTime Date_e)
        {
            if (Report.Count != 0)
            {
                sfd.FileName = $"Запросы медицинской помощи СМЭВ с {Date_b.ToShortDateString()} по {Date_e.ToShortDateString()}";
                if (sfd.ShowDialog() == true)
                {
                    var excel = new ExcelOpenXML(sfd.FileName, "Данные");
                    var styleDef = excel.CreateType(new FontOpenXML(), new BorderOpenXML(), null);
                    var styleBold = excel.CreateType(new FontOpenXML {Bold = true}, new BorderOpenXML(), null);
                    var styleDt = excel.CreateType(new FontOpenXML {Format = Convert.ToUInt32(DefaultNumFormat.F14)}, new BorderOpenXML(), null);
                    uint RowIndex = 1;
                    var row = excel.GetRow(RowIndex);
                    excel.PrintCell(row, 1, "Дата", styleBold);
                    excel.PrintCell(row, 2, "Кол-во запросов", styleBold);
                    excel.PrintCell(row, 3, "Кол-во людей", styleBold);
                    excel.PrintCell(row, 4, "Кол-во возвращенных услуг", styleBold);
                    excel.PrintCell(row, 5, "Отвечено", styleBold);
                    excel.PrintCell(row, 6, "Ошибок", styleBold);
                    excel.PrintCell(row, 7, "Без ответа", styleBold);
                    foreach (var r in Report)
                    {
                        RowIndex++;
                        row = excel.GetRow(RowIndex);

                        var curr_style = r.dt.HasValue ? styleDef : styleBold;
                        if (r.dt.HasValue)
                            excel.PrintCell(row, 1, r.dt.Value, styleDt);
                        else
                            excel.PrintCell(row, 1, "Итого", curr_style);

                        excel.PrintCell(row, 2, Convert.ToDouble(r.Count), curr_style);
                        excel.PrintCell(row, 3, Convert.ToDouble(r.People), curr_style);
                        excel.PrintCell(row, 4, Convert.ToDouble(r.USL), curr_style);
                        excel.PrintCell(row, 5, Convert.ToDouble(r.Answer), curr_style);
                        excel.PrintCell(row, 6, Convert.ToDouble(r.Error), curr_style);
                        excel.PrintCell(row, 7, Convert.ToDouble(r.noAnswer), curr_style);
                    }

                    excel.AutoSizeColumns(1, 7);
                    excel.Save();
                    if (MessageBox.Show("Формирование счета завершено. Показать файл?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        ShowSelectedInExplorer.FileOrFolder(sfd.FileName);
                    }
                }
            }
        }
    }
}


