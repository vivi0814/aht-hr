using ClosedXML.Excel;
using System.Data;

namespace HR_WorkFlow.Services
{
    public class Excel
    {
        public byte[] ExportToExcel(DataSet ds)
        {
            using (var workbook = new XLWorkbook())
            {
                foreach (DataTable dt in ds.Tables)
                {
                    var worksheet = workbook.Worksheets.Add(dt.TableName);
                    var currentRow = 1;
                    
                    // Write column headers
                    foreach (DataColumn column in dt.Columns)
                    {
                        worksheet.Cell(currentRow, column.Ordinal + 1).Value = column.ColumnName;
                    }
                    
                    // Write data rows
                    foreach (DataRow row in dt.Rows)
                    {
                        currentRow++;
                        for (var i = 0; i < dt.Columns.Count; i++)
                        {
                            worksheet.Cell(currentRow, i + 1).Value = row[i].ToString();
                        }
                    }
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        public DataSet ImportFromExcel(byte[] file)
        {
            using (var stream = new MemoryStream(file))
            {
                using (var workbook = new XLWorkbook(stream))
                {
                    var ds = new DataSet();
                    foreach (var worksheet in workbook.Worksheets)
                    {
                        var dt = new DataTable(worksheet.Name);
                        var currentRow = 1;
                        
                        // Read column headers
                        foreach (var cell in worksheet.Rows().First().Cells())
                        {
                            dt.Columns.Add(cell.Value.ToString());
                        }
                        
                        // Read data rows
                        foreach (var row in worksheet.Rows().Skip(1))
                        {
                            currentRow++;
                            var dr = dt.NewRow();
                            for (var i = 0; i < row.Cells().Count(); i++)
                            {
                                dr[i] = row.Cell(i + 1).Value;
                            }
                            dt.Rows.Add(dr);
                        }
                        
                        ds.Tables.Add(dt);
                    }
                    return ds;
                }
            }
        }
    }
}
