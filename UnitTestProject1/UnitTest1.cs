using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using ExpressAll;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            string connectionString = "Data Source=HGH1-DEV01.qqtoa.com;Initial Catalog=CpsPayment;Persist Security Info=True;User ID=ReadOnlyUser;Password=ReadOnly@1234";
            using (IDbConnection conn = new SqlConnection(connectionString))
            {
                DateTime dt1 = DateTime.Now;
                conn.Open();
                List<Test1> merchants = conn.Query<Test1>(
                        //                   "SELECT *  FROM [CpsPayment].[dbo].[RedDotMerchant]").AsList();
                        "SELECT *  FROM [CpsMain].[dbo].[Test1]").AsList();
                DateTime dt2 = DateTime.Now;

                TimeSpan re = dt2 - dt1;
                Assert.IsNotNull(merchants);
                Assert.AreNotEqual(0, merchants.Count);
            }
        }
        [TestMethod]
        public void TestMethodTrans()
        {
            string connectionString = "Data Source=HGH1-DEV01.qqtoa.com;Initial Catalog=CpsPayment;Persist Security Info=True;User ID=ReadOnlyUser;Password=ReadOnly@1234";
            using (IDbConnection conn = new SqlConnection(connectionString))
            {
                IDbTransaction transaction = conn.BeginTransaction();
                try
                {
                    DateTime dt1 = DateTime.Now;
                    conn.Execute("insert into [CpsPayment].[dbo].[Test1] (OrderId,datecreated) values(@OrderId,@DateCreated)  ", new { OrderId = "1", DateCreated = dt1 }, transaction);
                    conn.Execute("insert into [CpsPayment].[dbo].[Test1] (OrderId,datecreated) values(@OrderId,@DateCreated)  ", new { OrderId = "2", DateCreated = dt1 }, transaction);
                    transaction.Commit();
                }catch
                {
                    transaction.Rollback();
                }

            }
        }
        [TestMethod]
        public void TestMethod2()
        {
            string connectionString = "Data Source=HGH1-DEV01.qqtoa.com;Initial Catalog=CpsPayment;Persist Security Info=True;User ID=ReadOnlyUser;Password=ReadOnly@1234";
            using (IDbConnection conn = new SqlConnection(connectionString))
            {
                DateTime dt1 = DateTime.Now;
                conn.Open();
                var command = conn.CreateCommand();
                // command.CommandText = "SELECT *  FROM [CpsPayment].[dbo].[RedDotMerchant]";
                command.CommandText = "SELECT  *  FROM [CpsMain].[dbo].[Test1]";
                
                List<Test1> merchants = command.ExecuteReader().TransformTo<Test1>();
                DateTime dt2 = DateTime.Now;

                TimeSpan re = dt2 - dt1;

                Assert.IsNotNull(merchants);
                Assert.AreNotEqual(0, merchants.Count);
            }
        }
        [TestMethod]
        public void TestMethod3()
        {
            var ss = ExpressAll.SqlMapper.Select<Test1>(t=>t.Select(s=>s.OrderId,s=>s.Mode),s=>s.OrderId=="11");

            List<Test1> test1s = ExpressAll.SqlMapper.Select<Test1>(t => t.Select(s => s.OrderId, s => s.Mode), s => s.OrderId == "11");


        }
        
        public class RedDotMerchant
        {
            /// <summary>
            /// 商户ID
            /// </summary>
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public long MerchantId { get; set; }

            /// <summary>
            /// 红点商户ID
            /// </summary>
            public string RedDotMerchantId { get; set; }

            public string ApiMode { get; set; }

            /// <summary>
            /// 商户名称
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// 商户货币
            /// </summary>
            public string Currency { get; set; }

            /// <summary>
            /// 红点发送的key
            /// </summary>
            public string MerchantKey { get; set; }

            /// <summary>
            /// 红点发送的SecretKey
            /// </summary>
            public string SecretKey { get; set; }

            public bool IsPublic { get; set; }

            public bool IsOnline { get; set; }

            public string Remark { get; set; }

            public decimal? TodayAmount { get; set; }

            public int? TodayCount { get; set; }

            public DateTime DateCreated { get; set; }

            public DateTime LastUpdate { get; set; }
        }
        [Table("_Test1")]
        public class Test1
        {
            /// <summary>
            /// 交易号
            /// </summary>
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public long TransactionId { get; set; }

            /// <summary>
            /// 集成模式，参考 <see cref="IntegrateMode"/>。
            /// </summary>
            public string Mode { get; set; }

            /// <summary>
            /// 订单编号
            /// </summary>
            public string OrderId { get; set; }

            /// <summary>
            /// 商户编号
            /// </summary>
            public int MerchantId { get; set; }

            /// <summary>
            /// 网站编号
            /// </summary>
            public int ApplicationId { get; set; }

            /// <summary>
            /// 网关编号
            /// </summary>
            public int? GatewayId { get; set; }

            /// <summary>
            /// 域名
            /// </summary>
            public string Domain { get; set; }

            /// <summary>
            /// 电子邮箱
            /// </summary>
            public string Email { get; set; }

            /// <summary>
            /// 用户 IP 地址
            /// </summary>
            public string IPAddress { get; set; }

            /// <summary>
            /// 语言
            /// </summary>
            public string Language { get; set; }

            /// <summary>
            /// 交易货币
            /// </summary>
            public string Currency { get; set; }

            /// <summary>
            /// 原始交易金额
            /// </summary>
            public decimal Amount { get; set; }

            /// <summary>
            /// 运费
            /// </summary>
            public decimal Freight { get; set; }

            /// <summary>
            /// 折扣
            /// </summary>
            public decimal Discount { get; set; }

            /// <summary>
            /// 税
            /// </summary>
            public decimal Tax { get; set; }

            /// <summary>
            /// 服务费。
            /// </summary>
            public decimal ServiceFee { get; set; }

            /// <summary>
            /// 银行交易货币。
            /// </summary>
            public string TransactionCurrency { get; set; }

            /// <summary>
            /// 银行交易金额。
            /// </summary>
            public decimal? TransactionAmount { get; set; }

            public decimal? TransactionFee { get; set; }

            /// <summary>
            /// 状态，参见 <see cref="OrderStatus"/>。
            /// </summary>
            public string Status { get; set; }

            /// <summary>
            /// 标记
            /// </summary>
            public string Flag { get; set; }

            /// <summary>
            /// 订单备注
            /// </summary>
            public string Remark { get; set; }

            /// <summary>
            /// 是否已删除?
            /// </summary>
            public bool Deleted { get; set; }

            /// <summary>
            /// 信用卡类型，参见 <see cref="CreditCardType"/>。
            /// </summary>
            public string CreditCardType { get; set; }

            /// <summary>
            /// 订单支付成功时间
            /// </summary>
            public DateTime? SuccessTime { get; set; }

            /// <summary>
            /// 原因，参见 <see cref="ReasonCode"/>。
            /// </summary>
            public string ReasonCode { get; set; }

            /// <summary>
            /// 运单状态，参见 <see cref="WaybillStatus"/>。
            /// </summary>
            public string WaybillStatus { get; set; }

            /// <summary>
            /// 创建时间
            /// </summary>
            public DateTime DateCreated { get; set; }

            /// <summary>
            /// 最后修改时间
            /// </summary>
            public DateTime LastUpdate { get; set; }
        }
    }
}
