using CfdiService.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;

namespace CfdiService
{
    public class ModelDbContext : DbContext
    {
        public ModelDbContext(string connectString)
            : base(connectString)
        {
        }

        public ModelDbContext() : base("name=CfdiConnection")
        {
            Database.SetInitializer<ModelDbContext>(new CreateDatabaseIfNotExists<ModelDbContext>());
            //Database.SetInitializer<ModelDbContext>(new DropCreateDatabaseIfModelChanges<ModelDbContext>());
        }

        public Company FindCompanyByRfc(string rfc)
        {
            var company = Companies
                .Where(c => c.CompanyRFC == rfc)
                .FirstOrDefault();

            return company;
        }

        public Client FindClientByRfc(string rfc)
        {
            var clientCompany = Clients
                .Where(c => c.ClientCompanyRFC == rfc)
                .FirstOrDefault();

            return clientCompany;
        }

        public Employee FindEmployeeByRfc(string rfc)
        {
            var employee = Employees
                .Where(e => e.RFC == rfc)
                .FirstOrDefault();

            return employee;
        }

        public Employee FindEmployeeByCurp(string curp)
        {
            var employee = Employees
                .Where(e => e.CURP == curp)
                .FirstOrDefault();

            return employee;
        }

        virtual public DbSet<Batch> Batches { get; set; }
        virtual public DbSet<Company> Companies { get; set; }
        virtual public DbSet<Document> Documents { get; set; }
        virtual public DbSet<Employee> Employees { get; set; }
        virtual public DbSet<SignaturePurchase> SignaturePurchases { get; set; }
        virtual public DbSet<User> Users { get; set; }
        virtual public DbSet<SystemSettings> Settings { get; set; }
        virtual public DbSet<EmployeeSecurityQuestions> SecurityQuestions { get; set; }
        virtual public DbSet<EmployeesCode> EmployeeSecurityCodes { get; set; }
        virtual public DbSet<Client> Clients { get; set; }
        virtual public DbSet<ClientUser> ClientUsers { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Document>()
                .HasOptional<Batch>(d => d.Batch)
                .WithMany()
                .WillCascadeOnDelete(false);
        }
    }
}