﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EFDBFirst
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // tum urunleri listeleyelim
            NorthwindSabahEntities db = new NorthwindSabahEntities();
            // product uzerinden ilerlendigi icin tum productlar gozukmeli yani left join
            var sorgu1 = db.Products
                .Select(x => new
                {
                    x.ProductName,
                    x.UnitPrice,
                    x.Category.CategoryName
                })
                .ToList();
            //burada iliski kuruldugu icin iliskideki kurala gore inner join calisir.
            var sorgu2 = from p in db.Products
                         join cat in db.Categories on p.CategoryID equals cat.CategoryID
                         select new
                         {
                             UrunAdi = p.ProductName,
                             Fiyat = p.UnitPrice,
                             Kategori = cat.CategoryName
                         };

            dgvTest.DataSource = sorgu2.ToList();

            //calisanlarimi email adresleri ile listeleyin

            var sorgu3 = db.Employees
                .Select(x => new
                {
                    x.FirstName,
                    x.LastName,
                    Email = (x.FirstName.Substring(0, 1) + x.LastName + "@northwind.com").ToLower()
                }).ToList();
            dgvTest.DataSource = sorgu3;
            var sorgu4 = from emp in db.Employees
                         select new
                         {
                             emp.FirstName,
                             emp.LastName,
                             Email = (emp.FirstName.Substring(0, 1) + emp.LastName + "@northwind.com").ToLower()
                         };

            dgvTest.DataSource = sorgu4.ToList();

            this.Text = $"{db.Products.Average(x => x.UnitPrice):c2}";

            var sorgu5 = db.Products
                .Where(x => x.UnitPrice >= db.Products.Average(y => y.UnitPrice))
                .Select(x => new
                {
                    x.ProductName,
                    Fiyat = x.UnitPrice,
                    x.Category.CategoryName
                })
                .OrderByDescending(x => x.Fiyat)
                .ToList();
            dgvTest.DataSource = sorgu5;

            var sorgu6 = from p in db.Products
                         where p.UnitPrice >= db.Products.Average(x => x.UnitPrice)
                         orderby p.UnitPrice descending
                         select new
                         {
                             p.ProductName,
                             Fiyat = p.UnitPrice,
                             p.Category.CategoryName
                         };

            dgvTest.DataSource = sorgu6.ToList();

            //hangi kategoriden kac tane urunum var

            var sorgu7 = db.Products
                .Where(x => x.CategoryID.HasValue && x.SupplierID.HasValue)
                .GroupBy(x => new { x.Category.CategoryName, x.Supplier.CompanyName })
                .Select(x => new
                {
                    CategoryName = x.Key.CategoryName,
                    CompanyName = x.Key.CompanyName,
                    Total = x.Count()
                })
                .OrderBy(x => x.CategoryName)
                .ThenBy(x => x.CompanyName)
                .ToList();
            dgvTest.DataSource = sorgu7;

            var sorgu8 = from product in db.Products
                         join category in db.Categories on product.CategoryID equals category.CategoryID
                         join supp in db.Suppliers on product.SupplierID equals supp.SupplierID
                         group new
                         {
                             category,
                             supp
                         } by new
                         {
                             category.CategoryName,
                             supp.CompanyName
                         }
                into gp
                         orderby gp.Key.CategoryName ascending, gp.Key.CategoryName ascending
                         select new
                         {
                             CategoryName = gp.Key.CategoryName,
                             CompanyName = gp.Key.CompanyName,
                             Total = gp.Count()
                         };
            dgvTest.DataSource = sorgu8.ToList();

            // hangi urunden ne kadarlik siparis verilmis (tl bazinda)

            var sorgu9 = db.Order_Details
                .Join(db.Products,
                od => od.ProductID,
                product => product.ProductID,
                (od, product) => new { od, product })
                .GroupBy(x => x.product.ProductName)
                .OrderBy(x => x.Key)
                .ToList()
                .Select(x => new
                {
                    x.Key,
                    Total = Math.Round(x.Sum(y => y.od.UnitPrice * y.od.Quantity * Convert.ToDecimal(1 - y.od.Discount)), 2)
                });

            dgvTest.DataSource = sorgu9.ToList();

            var sorgu10 = from prod in db.Products
                          join od in db.Order_Details on prod.ProductID equals od.ProductID
                          group new
                          {
                              prod,
                              od
                          } by new
                          {
                              prod.ProductName
                          }
                into gp
                          orderby gp.Key.ProductName
                          select new
                          {
                              gp.Key.ProductName,
                              Total = gp.Sum(x => x.od.UnitPrice * x.od.Quantity)
                          };

            dgvTest.DataSource = sorgu10.ToList();

            //var data = sorgu10.ToList();


            //dgvTest.DataSource = data.GroupBy(x => x.ProductName)
            //    .Select(x => new
            //    {
            //        ProductName = x.Key,
            //        Total = Math.Round(x.Sum(y => y.od.UnitPrice * y.od.Quantity * Convert.ToDecimal(1 - y.od.Discount)), 2)
            //    }).ToList();

            // Çalışanlar kaç sipariş almış?
            var sorgu11 = from dbEmployee in db.Employees
                          join dbOrder in db.Orders on dbEmployee.EmployeeID equals dbOrder.EmployeeID
                          join dbOrderDetails in db.Order_Details on dbOrder.OrderID equals dbOrderDetails.OrderID
                          group new
                          {
                              dbEmployee,
                              dbOrderDetails
                          } by new
                          {
                              dbEmployee.FirstName,
                              dbEmployee.LastName
                          } into gp
                          select new
                          {
                              Employee = gp.Key.FirstName + " " + gp.Key.LastName,
                              Total = gp.Sum(x => x.dbOrderDetails.Quantity)
                          };
            dgvTest.DataSource = sorgu11.ToList();

            var sorgu12 = db.Order_Details
                .Join(db.Orders,
                od => od.OrderID,
                o => o.OrderID,
                (od, o) => new { od, o })
                .Join(db.Employees,
                gg => gg.o.EmployeeID,
                emp => emp.EmployeeID,
                (gg, emp) => new { gg, emp })
                .GroupBy(x => x.emp.FirstName + " " + x.emp.LastName)
                .Select(x => new
                {
                    Employee = x.Key,
                    Total = x.Sum(y => y.gg.od.Quantity)
                })
                .OrderByDescending(x => x.Total)
                .ToList();
            dgvTest.DataSource = sorgu12;

            // Hangi kategoriden toplam kaç adet sipariş var?
            var sorgu13 = from dbCategories in db.Categories
                          join dbProducts in db.Products on dbCategories.CategoryID equals dbProducts.CategoryID
                          join dbOrderDetails in db.Order_Details on dbProducts.ProductID equals dbOrderDetails.ProductID
                          group new
                          {
                              dbCategories,
                              dbOrderDetails
                          } by new
                          {
                              dbCategories.CategoryName
                          } into gp
                          select new
                          {
                              Category = gp.Key.CategoryName,
                              Total = gp.Sum(x => x.dbOrderDetails.Quantity)
                          };
            dgvTest.DataSource = sorgu13.ToList();

            // Sipariş no - toplam sipariş tutarı
            var sorgu14 = from orderDetail in db.Order_Details
                          group new
                          {
                              orderDetail
                          } by new
                          {
                              orderDetail.OrderID
                          } into gp
                          select new
                          {
                              gp.Key.OrderID,
                              Total = gp.Sum(x => x.orderDetail.UnitPrice * x.orderDetail.Quantity)
                          };

            var sorgu15 = db.Order_Details
                .GroupBy(x => x.OrderID)
                .ToList()
                .Select(x => new
                {
                    x.Key,
                    Total = $"{x.Sum(y => y.Quantity * y.UnitPrice * Convert.ToDecimal(1 - y.Discount)):c2}"
                }).ToList();
            dgvTest.DataSource = sorgu15;

            // Çalışanların hangi kategoriden kaç tane sipariş verdiği
            var sorgu16 = from dbCategories in db.Categories
                          join dbProducts in db.Products on dbCategories.CategoryID equals dbProducts.CategoryID
                          join dbOrderDetails in db.Order_Details on dbProducts.ProductID equals dbOrderDetails.ProductID
                          join dbEmployees in db.Employees on dbOrderDetails.Order.EmployeeID equals dbEmployees.EmployeeID
                          group new
                          {
                              dbEmployees,
                              dbCategories,
                              dbOrderDetails
                          } by new
                          {
                              dbEmployees.FirstName,
                              dbEmployees.LastName,
                              dbCategories.CategoryName
                          } into gp
                          orderby gp.Sum(x => x.dbOrderDetails.Quantity) descending
                          select new
                          {
                              Employee = gp.Key.FirstName + " " + gp.Key.LastName,
                              Category = gp.Key.CategoryName,
                              Total = gp.Sum(x => x.dbOrderDetails.Quantity)
                          };
            dgvTest.DataSource = sorgu16.ToList();
        }
    }
}