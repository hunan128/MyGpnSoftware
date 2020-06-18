using System;
using System.Collections.Generic;
using System.Text;
using NPOI;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Forms;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;


namespace MyGpnSoftware
{
    class NPOIExcel
    {
        public void ExportExcel(string fileName, DataGridView dgv)
        {
            if (dgv.Rows.Count == 0)
            {
                MessageBox.Show("请先导入「网管导出的表格」，然后再次尝试");
                return;
            }
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Excel 2003格式|*.xls";
            sfd.FileName = DateTime.Now.ToString("yyyy-MM-dd")+"批量保存表格";
            if (sfd.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            HSSFWorkbook wb = new HSSFWorkbook();
            HSSFSheet sheet = (HSSFSheet)wb.CreateSheet(fileName);
            HSSFRow headRow = (HSSFRow)sheet.CreateRow(0);
            for (int i = 0; i < dgv.Columns.Count; i++)
            {
                HSSFCell headCell = (HSSFCell)headRow.CreateCell(i, CellType.String);
                headCell.SetCellValue(dgv.Columns[i].HeaderText);
            }
            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                HSSFRow row = (HSSFRow)sheet.CreateRow(i + 1);
                for (int j = 0; j < dgv.Columns.Count; j++)
                {
                    HSSFCell cell = (HSSFCell)row.CreateCell(j);
                    if (dgv.Rows[i].Cells[j].Value == null)
                    {
                        cell.SetCellType(CellType.Blank);
                    }
                    else
                    {

                        cell.SetCellValue(dgv.Rows[i].Cells[j].Value.ToString());
                    }

                }

            }
            for (int i = 0; i < dgv.Columns.Count; i++)
            {
                sheet.AutoSizeColumn(i);
            }
            using (FileStream fs = new FileStream(sfd.FileName, FileMode.Create))
            {
                wb.Write(fs);
            }
            MessageBox.Show("导出成功！", "导出提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            wb.Close();
        }
    }
}
