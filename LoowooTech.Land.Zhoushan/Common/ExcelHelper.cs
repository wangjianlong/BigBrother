﻿using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoowooTech.Land.Zhoushan.Common
{
    public class ExcelCell
    {
        public ExcelCell()
        {
            Rowspan = 1;
            Colspan = 1;
        }

        public int Row { get; set; }

        public int Column { get; set; }

        public int Rowspan { get; set; }

        public int Colspan { get; set; }

        public object Value { get; set; }

        public bool Cover(int row, int column)
        {

            return Row <= row && (Row + Rowspan - 1) >= row && (Column + Colspan - 1 >= column) && Column <= column;
        }

        /// <summary>
        /// 计算Cell的rowspan和colspan
        /// </summary>
        /// <param name="list"></param>
        public void ComputeSpace(ICell cell, List<ExcelCell> list)
        {
            //如果是合并的单元格，则需要计算该单元格的rowspan和colspan
            if (cell.IsMergedCell && cell.CellType != CellType.Blank)
            {
                //先向右查询
                for (var i = cell.ColumnIndex + 1; i < cell.Row.LastCellNum; i++)
                {
                    var c = cell.Row.GetCell(i);
                    if (c != null && c.IsMergedCell && c.CellType == CellType.Blank)
                    {
                        //判断list中是否有Cell已经占用该单元格，如果占用，则不计算
                        var t = list.FirstOrDefault(e => e.Cover(c.RowIndex, c.ColumnIndex));
                        if (list.Any(e => e.Cover(c.RowIndex, c.ColumnIndex)))
                        {
                            break;
                        }
                        Colspan++;
                    }
                    else
                    {
                        break;
                    }
                }
                //向下查询
                for (var i = cell.RowIndex + 1; i < cell.Sheet.LastRowNum; i++)
                {
                    var row = cell.Sheet.GetRow(i);
                    if (row != null)
                    {
                        var c = row.GetCell(cell.ColumnIndex);
                        if (c != null && c.IsMergedCell && c.CellType == CellType.Blank)
                        {
                            //判断list中是否有Cell已经占用该单元格，如果占用，则不计算
                            if (list.Any(e => e.Cover(c.RowIndex, c.ColumnIndex)))
                            {
                                break;
                            }
                            Rowspan++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }

    public class ExcelHelper
    {
        private static ExcelCell ConvertToExcelCell(ICell cell)
        {
            if (cell == null) return null;
            var result = new ExcelCell();
            switch (cell.CellType)
            {
                case CellType.Numeric:
                    result.Value = cell.NumericCellValue;
                    break;
                case CellType.Formula:
                case CellType.Blank:
                case CellType.Error:
                case CellType.Unknown:
                    break;
                default:
                    result.Value = cell.StringCellValue;
                    break;
            }
            result.Row = cell.RowIndex;
            result.Column = cell.ColumnIndex;
            return result;
        }

        public static List<ExcelCell> ReadData(string filePath, int sheetIndex = 0)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                return ReadData(fs, sheetIndex);
            }
        }

        public static List<ExcelCell> ReadData(Stream stream, int sheetIndex = 0)
        {
            var workbook = WorkbookFactory.Create(stream);
            var sheet = workbook.GetSheetAt(sheetIndex);
            var list = new List<ExcelCell>();
            for (var i = sheet.FirstRowNum; i <= sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);
                if (row == null) continue;
                foreach (var cell in row.Cells)
                {
                    if (cell.CellType == CellType.Blank)
                    {
                        continue;
                    }
                    var excelCell = ConvertToExcelCell(cell);
                    excelCell.ComputeSpace(cell, list);
                    list.Add(excelCell);
                }
            }
            stream.Close();
            stream.Dispose();
            return list;
        }

        public static Stream WriteData(string filePath, List<ExcelCell> data, int sheetIndex = 0)
        {
            var workbook = WorkbookFactory.Create(filePath);
            var sheet = workbook.GetSheetAt(sheetIndex);
            foreach (var item in data)
            {
                var row = sheet.GetRow(item.Row);
                var cell = row.GetCell(item.Column);
                cell.SetCellValue(item.Value.ToString());
            }
            var result = new MemoryStream();
            workbook.Write(result);
            return result;
        }
    }
}
