using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using static TeachAnnouncement.Models.AdmAnnoViewModel;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.Data;
using Dapper;
using System.Text;

namespace TeachAnnouncement.Controllers
{
    public class AdmAnnoController : Controller
    {
        private readonly IConfiguration _configuration;

        #region 頁面載入動作
        public AdmAnnoController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IActionResult Index()
        {
            return View();
        }
        #endregion

        #region 查詢相關
        /// <summary>
        /// 查詢公告
        /// </summary>
        /// <param name="inModel"></param>
        /// <returns></returns>
        public IActionResult Query(QueryIn inModel)
        {
            QueryOut outModel = new QueryOut();
            outModel.Grid = new List<AnnoModel>();

            // 資料庫連線字串
            string connStr = _configuration.GetConnectionString("SqlServer");
            using (var cn = new SqlConnection(connStr))
            {
                // 主要查詢 SQL
                string sql = @"SELECT Pkey, CONVERT(varchar(12) , AnnoDate, 111 ) as AnnoDate, AnnoSubject, AnnoContent, AnnoStatus, Case AnnoStatus when '1' then '顯示' when '0' then '隱藏' end As AnnoStatusName
                                FROM Announcement 
                                WHERE 1=1 ";

                if (!string.IsNullOrEmpty(inModel.AnnoSubject))
                {
                    sql += " AND AnnoSubject LIKE @AnnoSubject ";
                }
                if (!string.IsNullOrEmpty(inModel.AnnoStatus))
                {
                    sql += " AND AnnoStatus = @AnnoStatus ";
                }
                sql += " ORDER BY AnnoDate desc, AnnoStatus ";

                object param = new
                {
                    AnnoSubject = "%" + inModel.AnnoSubject + "%",
                    AnnoStatus = inModel.AnnoStatus
                };

                // 分頁處理
                int totalRowCount = 0;
                if (inModel.pagination.pageNo > 0)
                {
                    string orderBy = "";
                    // 取得總筆數
                    string totalRowSql = sql;
                    if (totalRowSql.ToUpper().IndexOf("ORDER BY") > -1)
                    {
                        orderBy = totalRowSql.Substring(sql.ToUpper().LastIndexOf("ORDER BY"));
                        totalRowSql = totalRowSql.Replace(orderBy, "");
                    }
                    totalRowSql = "SELECT COUNT(*) AS CNT FROM (" + totalRowSql + ") CNT_TABLE";
                    var rowCnt = cn.Query(totalRowSql, param);
                    foreach (var item in rowCnt)
                    {
                        totalRowCount = item.CNT;
                    }

                    // 取得分頁 SQL
                    int startRow = ((inModel.pagination.pageNo - 1) * inModel.pagination.pageSize) + 1;
                    int endRow = (startRow + inModel.pagination.pageSize) - 1;
                    orderBy = sql.Substring(sql.ToString().ToUpper().LastIndexOf("ORDER BY"));
                    sql = sql.Replace(orderBy, "");
                    // 去除 Order by 別名
                    orderBy = orderBy.ToUpper().Replace("ORDER BY", "");
                    StringBuilder newOrderBy = new StringBuilder();
                    int index = 0;
                    string[] orderBys = orderBy.Split(',');
                    for (int i = 0; i < orderBys.Length; i++)
                    {
                        if (newOrderBy.Length > 0) { newOrderBy.Append(","); }
                        string ob = orderBys[i];
                        index = ob.IndexOf('.');
                        if (index > -1)
                        {
                            newOrderBy.Append(ob.Substring(index + 1));
                        }
                        else
                        {
                            newOrderBy.Append(ob);
                        }
                    }
                    newOrderBy.Insert(0, "ORDER BY ");

                    sql = string.Concat(
                        new object[] {
                            "SELECT * FROM (SELECT *, ROW_NUMBER() OVER (", newOrderBy.ToString(), ") AS RCOUNT FROM (", sql, ") PAGE_SQL ) PAGE_SQL2 WHERE PAGE_SQL2.RCOUNT BETWEEN "
                            , startRow, " AND ", endRow, " ", newOrderBy.ToString() });
                }

                // 使用 Dapper 查詢
                var list = cn.Query<AnnoModel>(sql, param);

                // 輸出物件
                foreach (var item in list)
                {
                    outModel.Grid.Add(item);
                }

                // 計算分頁
                outModel.pagination = this.PreparePage(inModel.pagination, totalRowCount);
            }
            return Json(outModel);
        }

        /// <summary>
        /// 計算分頁
        /// </summary>
        /// <param name="model"></param>
        /// <param name="TotalRowCount"></param>
        /// <returns></returns>
        public PaginationModel PreparePage(PaginationModel model, int TotalRowCount)
        {
            List<string> pages = new List<string>();
            int pageStart = ((model.pageNo - 1) / 10) * 10;
            model.totalCount = TotalRowCount;
            model.totalPage =
                    Convert.ToInt16(Math.Ceiling(
                     double.Parse(model.totalCount.ToString()) / double.Parse(model.pageSize.ToString())
                    ));

            if (model.pageNo > 10)
                pages.Add("<<");
            if (model.pageNo > 1)
                pages.Add("<");
            for (int i = 1; i <= 10; ++i)
            {
                if (pageStart + i > model.totalPage)
                    break;
                pages.Add((pageStart + i).ToString());
            }
            if (model.pageNo < model.totalPage)
                pages.Add(">");
            if ((pageStart + 10) < model.totalPage)
                pages.Add(">>");
            model.pages = pages;
            return model;
        }
        #endregion

        #region 新增相關
        /// <summary>
        /// 新增公告
        /// </summary>
        /// <param name="inModel"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        public IActionResult AddSave(AddSaveIn inModel)
        {
            AddSaveOut outModel = new AddSaveOut();

            // 檢查參數
            if (ModelState.IsValid == false)
            {
                outModel.ErrMsg = string.Join("\n", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                return Json(outModel);
            }

            // 資料庫連線字串
            string connStr = _configuration.GetConnectionString("SqlServer");

            using (var conn = new SqlConnection(connStr))
            {

                string sql = @"INSERT INTO Announcement(AnnoDate, AnnoSubject, AnnoContent, AnnoStatus)
							VALUES      (@AnnoDate, @AnnoSubject, @AnnoContent, @AnnoStatus)";
                var param = new
                {
                    AnnoDate = inModel.AnnoDate,
                    AnnoSubject = inModel.AnnoSubject,
                    AnnoContent = inModel.AnnoContent,
                    AnnoStatus = inModel.AnnoStatus
                };
                // 使用 Dapper
                conn.Execute(sql, param);
            }

            outModel.ResultMsg = "新增完成";

            return Json(outModel);
        }
        #endregion

        #region 修改相關
        /// <summary>
        /// 修改公告
        /// </summary>
        /// <param name="inModel"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        public IActionResult EditSave(EditSaveIn inModel)
        {
            EditSaveOut outModel = new EditSaveOut();

            // 檢查參數
            if (ModelState.IsValid == false)
            {
                outModel.ErrMsg = string.Join("\n", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                return Json(outModel);
            }

            // 資料庫連線字串
            string connStr = _configuration.GetConnectionString("SqlServer");

            using (var conn = new SqlConnection(connStr))
            {
                string sql = @"UPDATE Announcement
                                SET    AnnoDate = @AnnoDate, AnnoSubject = @AnnoSubject, AnnoContent = @AnnoContent, AnnoStatus = @AnnoStatus
                                WHERE  Pkey = @Pkey";
                var param = new
                {
                    AnnoDate = inModel.AnnoDate,
                    AnnoSubject = inModel.AnnoSubject,
                    AnnoContent = inModel.AnnoContent,
                    AnnoStatus = inModel.AnnoStatus,
                    Pkey = inModel.Pkey
                };

                // 使用 Dapper
                int ret = conn.Execute(sql, param);
                if (ret > 0)
                {
                    outModel.ResultMsg = "修改完成";
                }
            }
            return Json(outModel);
        }
        #endregion

        #region 刪除相關
        /// <summary>
        /// 刪除公告
        /// </summary>
        /// <param name="inModel"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        public IActionResult DelCheck(DelCheckIn inModel)
        {
            DelCheckOut outModel = new DelCheckOut();

            // 檢查參數
            if (inModel.checks.Count == 0)
            {
                outModel.ErrMsg = "缺少輸入資料";
                return Json(outModel);
            }

            // 資料庫連線字串
            string connStr = _configuration.GetConnectionString("SqlServer");

            using (var conn = new SqlConnection(connStr))
            {
                int ret = 0;
                foreach (AnnoModel model in inModel.checks)
                {
                    string sql = @"DELETE Announcement
                                WHERE  Pkey = @Pkey";
                    var param = new
                    {
                        Pkey = model.Pkey
                    };

                    // 使用 Dapper
                    ret += conn.Execute(sql, param);
                }

                if (ret > 0)
                {
                    outModel.ResultMsg = "成功刪除 " + ret + " 筆資料";
                }
            }
            return Json(outModel);
        }
        #endregion
    }
}
