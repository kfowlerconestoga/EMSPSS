using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Utilities
{
    public static class EMSPSSUtilities
    {
        static private EMSEntities12 db = new EMSEntities12();
        /* ---------------------------
        *	Name	: EMSPSSUtilities -- DateIsElapsed
        *
        *	Purpose :   Verfies the offset of a start date and end date is positive (and optionally
        *	            greater than an expected offset value)
        *	Inputs	:	DateTime startDate - the starting date to be checked
        *	            DateTime endDate - the ending date to be checked
        *	            (Optional) Int32 expOffset - expected offset between dates
        *	Outputs	:   None
        *	Returns	:	bool - Offset between startDate and endDate is positive (and, when expOffset
         *	                   is specified, greater than the expected offset)
        */
        public static bool DateIsElapsed(DateTime startDate, DateTime endDate, Int32 expOffset = 0)
        {
            if (endDate > startDate)
            {
                if (expOffset == 0)
                {
                    return true;
                }
                else
                {
                    Int32 offset = endDate.Year - startDate.Year;
                    if (offset < expOffset)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        static public string GetEmployeeType(int id)
        {
            String type = FindEmployeeType(id);

            if (type == "Full Time ")
            {
                return "FullTimeEmployee";
                
            }
            else if (type == "Part Time ")
            {
                return "PartTimeEmployee";
                
            }
            else if (type == "Seasonal  ")
            {
                return "SeasonalEmployee";
                
            }
            else if (type == "Contract  ")
            {
                return "ContractEmployee";                
            }
            return "";
        }

        static public int GetCompanyId(int id)
        {
            string type = GetEmployeeType(id);
            if (type == "FullTimeEmployee")
            {
                return GetFullTimer(id).EmployedWithId;
            }
            else if (type == "PartTimeEmployee")
            {
                return GetPartTimer(id).EmployedWith2Id;
            }
            else if (type == "SeasonalEmployee")
            {
                return GetSeasonal(id).EmployedWith3Id;
            }
            return 1;

        }

        //figure out which employee type we are looking at
        static private String FindEmployeeType(int id)
        {
            List<EmployeeType> etl = db.EmployeeTypes.ToList();
            String type = "";
            foreach (EmployeeType et in etl)
            {
                if (et.Employee.EmployeeId == id)
                {
                    type = et.EmployeeType1;
                    break;
                }
            }
            return type;
        }

        static public List<String> GetSeasonList()
        {
            var SeasonLst = new List<String>();
            var SeasonQry = from d in db.Seasons
                            orderby d.Season1
                            select d.Season1;
            SeasonLst.AddRange(SeasonQry.Distinct());
            return SeasonLst;
        }
        static public List<String> GetCompList()
        {
            var CompLst = new List<String>();
            var CompQry = from d in db.Companies
                          orderby d.CompanyName
                          select d.CompanyName;
            CompLst.AddRange(CompQry.Distinct());
            return CompLst;
        }
        
        // ----Params----
        // String OldVal == old value
        // String NewVal == new value
        // int FieldnameId == Id of the field name in the field name table
        // int auditTypeId == Id of the audit action in the Audit action table
        // int employeeId == Id of the employee being looked at
        // String Username == username of the person who did the action
        static public void CompareFields(String OldVal, String NewVal, int FieldnameId, int auditTypeId, int employeeId, String Username)
        {
            db = new EMSEntities12();
            Audit audit = new Audit();
            if (OldVal != NewVal)
            {
                audit.UserName = Username;
                audit.DateTime = DateTime.Now;
                audit.AuditTypeId = auditTypeId;
                audit.FieldId = FieldnameId;
                audit.OldValue = OldVal;
                audit.NewValue = NewVal;
                audit.EmployeeId = employeeId;
                db.Audits.Add(audit);
                db.SaveChanges();
            }
        }

        static public void AuditNewBaseEmployee(Employee newEmployee, String Username)
        {
            CompareFields("", newEmployee.FirstName, 3, 1, newEmployee.EmployeeId, Username);
            CompareFields("", newEmployee.LastName, 2, 1, newEmployee.EmployeeId, Username);
            CompareFields("", newEmployee.SIN_BN, 1, 1, newEmployee.EmployeeId, Username);
            CompareFields("", newEmployee.DateOfBirth.ToString(), 4, 1, newEmployee.EmployeeId, Username);

        }


        static public void AuditNewContractEmployee(ContractEmployee newEmployee, String Username)
        {
            CompareFields("", newEmployee.ContractStopDate.ToString(), 6, 1, newEmployee.Employee.EmployeeId, Username);
            CompareFields("", newEmployee.ReasonForLeaving4Id.ToString(), 7, 1, newEmployee.Employee.EmployeeId, Username);
            CompareFields("", newEmployee.FixedContractAmount.ToString(), 8, 1, newEmployee.Employee.EmployeeId, Username);
        }

        static public void AuditExistingContractEmployee(int existingEmpId, ContractEmployee newEmployee, String Username)
        {
            ContractEmployee ce = GetContract(existingEmpId);
            CompareFields(ce.ContractStopDate.ToString(), newEmployee.ContractStopDate.ToString(), 6, 2, newEmployee.Employee.EmployeeId, Username);
            CompareFields(ce.ReasonForLeaving4Id.ToString(), newEmployee.ReasonForLeaving4Id.ToString(), 7, 2, newEmployee.Employee.EmployeeId, Username);
            CompareFields(ce.FixedContractAmount.ToString(), newEmployee.FixedContractAmount.ToString(), 8, 2, newEmployee.Employee.EmployeeId, Username);
        }

        static public void AuditExistingSeasonalEmployee(int existingEmpId, SeasonalEmployee newEmployee, String Username)
        {
            SeasonalEmployee s = GetSeasonal(existingEmpId);
            CompareFields(s.PiecePay.ToString(), newEmployee.PiecePay.ToString(), 16, 2, newEmployee.Employee.EmployeeId, Username);
            CompareFields(s.ReasonForLeaving3Id.ToString(), newEmployee.ReasonForLeaving3Id.ToString(), 7, 2, newEmployee.Employee.EmployeeId, Username);
            CompareFields(s.SeasonId.ToString(), newEmployee.SeasonId.ToString(), 17, 2, newEmployee.Employee.EmployeeId, Username);
        }

        static public void AuditExistingPartTimeEmployee(int existingEmpId, PartTimeEmployee newEmployee, String Username)
        {
            PartTimeEmployee pt = GetPartTimer(existingEmpId);
            CompareFields(pt.DateOfTermination.ToString(), newEmployee.DateOfTermination.ToString(), 12, 3, newEmployee.Employee.EmployeeId, Username);
            CompareFields(pt.HourlyRate.ToString(), newEmployee.HourlyRate.ToString(), 15, 2, newEmployee.Employee.EmployeeId, Username);
            CompareFields(pt.ReasonForLeaving2Id.ToString(), newEmployee.ReasonForLeaving2Id.ToString(), 7, 2, newEmployee.Employee.EmployeeId, Username);
        }

        static public void AuditExistingFullTimeEmployee(int existingEmpId, FullTimeEmployee newEmployee, String Username)
        {
            FullTimeEmployee ft = GetFullTimer(existingEmpId);
            CompareFields(ft.DateOfTermination.ToString(), newEmployee.DateOfTermination.ToString(), 12, 3, newEmployee.Employee.EmployeeId, Username);
            CompareFields(ft.Salary.ToString(), newEmployee.Salary.ToString(), 13, 2, newEmployee.Employee.EmployeeId, Username);
            CompareFields(ft.ReasonForLeavingId.ToString(), newEmployee.ReasonForLeavingId.ToString(), 7, 2, newEmployee.Employee.EmployeeId, Username);
        }

        static public void AuditExistingEmployee(int existingEmpId, Employee newEmployee,String Username)
        {
            Employee existingEmp = db.Employees.Find(existingEmpId);
            CompareFields(existingEmp.FirstName, newEmployee.FirstName, 3, 2, existingEmp.EmployeeId, Username);
            CompareFields(existingEmp.LastName, newEmployee.LastName, 2, 2, existingEmp.EmployeeId, Username);
            CompareFields(existingEmp.SIN_BN.ToString(), newEmployee.SIN_BN.ToString(), 1, 2, existingEmp.EmployeeId, Username);
            CompareFields(existingEmp.DateOfBirth.ToString(), newEmployee.DateOfBirth.ToString(), 4, 2, existingEmp.EmployeeId, Username);
        }



        //get a full time employee based on id
        static public FullTimeEmployee GetFullTimer(int id)
        {
            db = new EMSEntities12();
            FullTimeEmployee fulltimer = new FullTimeEmployee();
            List<FullTimeEmployee> ftl = db.FullTimeEmployees.ToList();
            foreach (FullTimeEmployee ft in ftl)
            {
                if (ft.EmployeeRefId == id)
                {
                    fulltimer = ft;
                    break;
                }
            }
            return fulltimer;
        }

        //get a part time employee based on id
        static public PartTimeEmployee GetPartTimer(int id)
        {
            db = new EMSEntities12();
            PartTimeEmployee parttimer = new PartTimeEmployee();
            List<PartTimeEmployee> ptl = db.PartTimeEmployees.ToList();
            foreach (PartTimeEmployee pt in ptl)
            {
                if (pt.EmployeeRef2Id == id)
                {
                    parttimer = pt;
                    break;
                }
            }
            return parttimer;
        }

        //get a part time employee based on id
        static public SeasonalEmployee GetSeasonal(int id)
        {
            db = new EMSEntities12();
            SeasonalEmployee seasonal = new SeasonalEmployee();
            List<SeasonalEmployee> sl = db.SeasonalEmployees.ToList();
            foreach (SeasonalEmployee s in sl)
            {
                if (s.EmployeeRef3Id == id)
                {
                    seasonal = s;
                    break;
                }
            }
            return seasonal;
        }

        //get a part time employee based on id
        static public ContractEmployee GetContract(int id)
        {
            db = new EMSEntities12();
            ContractEmployee contract = new ContractEmployee();
            List<ContractEmployee> cl = db.ContractEmployees.ToList();
            foreach (ContractEmployee c in cl)
            {
                if (c.EmployeeRef4Id == id)
                {
                    contract = c;
                    break;
                }
            }
            return contract;
        }

        static public Boolean ValidateEmployeeComplete(Employee e)
        {
            Boolean complete = true;

            if (e.FirstName == null)
            {
                complete = false;
            }
            else if (e.LastName == null)
            {
                complete = false;
            }
            else if (e.DateOfBirth == null)
            {
                complete = false;
            }
            else if (e.SIN_BN == null)
            {
                complete = false;
            }
            return complete;
        }

        static public Boolean ValidateFullTimeEmployeeComplete(FullTimeEmployee ft)
        {
            Boolean complete = ValidateEmployeeComplete(ft.Employee);
            
            if (ft.DateOfHire == null)
            {
                complete = false;
            }
            else if (ft.Salary <= 0 || ft.Salary == null)
            {
                complete = false;
            }
            return complete;
        }

        static public Boolean ValidatePartTimeEmployeeComplete(PartTimeEmployee pt)
        {
            Boolean complete = ValidateEmployeeComplete(pt.Employee);
            
            if (pt.DateOfHire == null)
            {
                complete = false;
            }
            else if (pt.HourlyRate <= 0 || pt.HourlyRate == null)
            {
                complete = false;
            }
            return complete;
        }

        static public Boolean ValidateContractEmployeeComplete(ContractEmployee c)
        {
            Boolean complete = ValidateEmployeeComplete(c.Employee);
           
            if (c.ContractStartDate == null)
            {
                complete = false;
            }
            else if (c.ContractStopDate == null)
            {
                complete = false;
            }
            else if (c.FixedContractAmount <= 0)
            {
                complete = false;
            }
            return complete;
        }

        static public Boolean ValidateSeasonalEmployeeComplete(SeasonalEmployee s)
        {
            Boolean complete = ValidateEmployeeComplete(s.Employee);

            if (s.SeasonYear == null)
            {
                complete = false;
            }
            else if (s.SeasonId == null)
            {
                complete = false;
            }
            else if (s.PiecePay <= 0 || s.PiecePay == null)
            {
                complete = false;
            }
            return complete;
        }

        /* ---------------------------
       *	Name	: EMSPSSUtilities -- ValidateSIN
       *
       *	Purpose :   Validates and formats the SIN number passed in
       *	Inputs	:	String sinNum - SIN number value to validate
       *	Outputs	:	None
       *	Returns	:	bool - SIN number valid/invalid
       */
        public static bool ValidateSIN(ref String sinNum)
        {
            bool valid;

            valid = SINValid(ref sinNum);
            if (valid)
            {
                valid = VerifySIN(sinNum);
            }

            return valid;
        }



        /* ---------------------------
       *	Name	: EMSPSSUtilities -- SINValid
       *
       *	Purpose :   Validates and formats the SIN number passed in
       *	Inputs	:	String sinNum - SIN number value to validate
       *	Outputs	:	None
       *	Returns	:	bool - SIN number valid/invalid
       */
        public static bool SINValid(ref String sinNum)
        {
            bool valid = false;

            if (sinNum.Contains(" "))
            {
                sinNum = sinNum.Replace(" ", String.Empty);
            }
            if (sinNum.Contains("-"))
            {
                sinNum = sinNum.Replace("-", String.Empty);
            }

            if (sinNum == String.Empty || sinNum.Length == 9)
            {
                valid = true; //Blank value or 9 digits found - valid
            }
            else
            {
                valid = false;
            }

            return valid;
        }



        /* ---------------------------
        *	Name	: EMSPSSUtilities -- VerifySIN
        *
        *	Purpose :   Verifies the SIN number passed in is real
        *	Inputs	:	String sinNum - SIN number value to verify
        *	Outputs	:	None
        *	Returns	:	bool - Value valid/invalid
        */
        public static bool VerifySIN(String sinNum)
        {
            bool allNumeric = false;
            bool valid = false;
            Int32[] tmpSIN = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            Int32[] buffer = { 0, 0, 0, 0, 0, 0, 0, 0 };
            Int32 evenNums = 0;
            Int32 oddNums = 0;
            Int32 sum = 0;

            if (sinNum.Length == 9)
            {
                for (Int32 x = 0; x < sinNum.Length; ++x)
                {
                    if (!Int32.TryParse(sinNum[x].ToString(), out tmpSIN[x])) //Non-numeric character found
                    {
                        valid = false;  //SIN number is invalid
                        allNumeric = false; //Not all characters are numeric, skip SIN check digit
                        break;
                    }
                    allNumeric = true; //All characters numeric, proceed with check digit
                }
                if (allNumeric)
                {
                    for (Int32 x = 1; x < 8; x += 2) //Add even placed digits to each other
                    {
                        tmpSIN[x] = tmpSIN[x] + tmpSIN[x];
                    }

                    Int32 y = 0;
                    for (Int32 x = 0; x < 7; ++x)
                    {
                        oddNums += tmpSIN[y]; //Add oddly placed numbers together
                        ++y;
                        buffer[x] = tmpSIN[y] / 10; //Take first digit of each evenly placed number
                        ++x;
                        buffer[x] = tmpSIN[y] % 10; //Take second digit of each evenly placed number
                        ++y;
                    }
                    oddNums += tmpSIN[8];   //Last oddly placed number (can't run loop long enough to include it there)

                    for (Int32 x = 0; x < 8; ++x)
                    {
                        evenNums += buffer[x];  //Cross add the evenly placed numbers
                    }

                    sum = oddNums + evenNums;   //Add evenly and oddly placed numbers together

                    if (sum % 10 == 0)
                    {
                        valid = true;   //Check digit and last digit in SIN are 0, SIN numbers is valid
                    }
                    else
                    {
                        Int32 tmp = ((sum / 10) + 1) * 10;  //Next highest number from sum divisible by 10
                        tmp = tmp - sum;    //Check digit

                        if (tmp == tmpSIN[8])
                        {
                            valid = true;   //Check digit matches last digit of SIN, SIN number is valid
                        }
                        else
                        {
                            valid = false;  //Check digit did not match last digit of SIN in either case, SIN number invalid
                        }
                    }
                }
            }
            return valid;
        }



        /* ---------------------------
        *	Name	:   EMSPSSUtilites -- VerifyBusinessNum
        *
        *	Purpose :   Validates the Business Number
        *	Inputs	:	String BusinessNum: a number to be validates
        *               String Date: the date to compare	    
        *	Outputs	:	Logs any exceptions with Logger
        *	Returns	:	bool - If the business number is valid
        */
        public static bool VerifyBusinessNum(String BusinessNum, String Date)
        {
            bool valid = false;
            String year = String.Empty;
            String DDate = String.Empty;

            if (BusinessNum != String.Empty && BusinessNum.Length == 9 && Date != String.Empty)
            {
                year = BusinessNum.Substring(0, 2);
                DDate = Date.Substring(2, 3);

                if (DDate.Contains(year))
                {
                    //if (VerifySIN(BusinessNum)) // check the check digit
                    //{
                    //    valid = true;//if business number is a string with 9 characters
                    //     if the string contains the year of incorporation
                    //}
                    valid = true;
                }
            }

            return valid;
        }

        public static String FormatSIN_BN(String sin_bn, bool isSin)
        {
            String newSIN_BN = sin_bn;

            if (isSin)
            {
                newSIN_BN = newSIN_BN.Insert(6, " ");
                newSIN_BN = newSIN_BN.Insert(3, " ");
            }
            else
            {
                newSIN_BN = newSIN_BN.Insert(5, " ");
            }

            return newSIN_BN;
        }

        public static void UpdateEmployeeStatuses()
        {
            List<EmployeeType> etl = db.EmployeeTypes.ToList();
            foreach (EmployeeType et in etl)
            {
                if (et.EmployeeType1 != "Contract  ")
                {
                    if (et.Employee.FullTimeEmployees.Count > 0)
                    {
                        FullTimeEmployee fulltimer = EMSPSSUtilities.GetFullTimer(et.EmployeeId);
                        if ((fulltimer.DateOfTermination > DateTime.Now || fulltimer.DateOfTermination == null) && fulltimer.Employee.Completed)
                        {
                            fulltimer.Employee.Active = true;
                            db.Entry(fulltimer.Employee).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else
                        {
                            fulltimer.Employee.Active = false;
                            db.Entry(fulltimer.Employee).State = EntityState.Modified;
                            db.SaveChanges();

                        }
                    }
                    else if (et.Employee.PartTimeEmployees.Count > 0)
                    {
                        PartTimeEmployee parttimer = EMSPSSUtilities.GetPartTimer(et.EmployeeId);
                        if ((parttimer.DateOfTermination > DateTime.Now || parttimer.DateOfTermination == null) && parttimer.Employee.Completed)
                        {
                            parttimer.Employee.Active = true;
                            db.Entry(parttimer.Employee).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else
                        {
                            parttimer.Employee.Active = false;
                            db.Entry(parttimer.Employee).State = EntityState.Modified;
                            db.SaveChanges();

                        }
                    }
                    else if (et.Employee.SeasonalEmployees.Count > 0)
                    {

                        SeasonalEmployee sea = EMSPSSUtilities.GetSeasonal(et.EmployeeId);
                        DateTime lowRange = DateTime.Now;
                        DateTime highRange = DateTime.Now;
                        if (sea.Season.Season1 == "Winter")
                        {
                            lowRange = new DateTime(DateTime.Now.Year - 1, 12, 1);
                            highRange = new DateTime(DateTime.Now.Year, 4, 30);
                        }
                        else if (sea.Season.Season1 == "Spring")
                        {
                            lowRange = new DateTime(DateTime.Now.Year, 5, 1);
                            highRange = new DateTime(DateTime.Now.Year, 6, 30);
                        }
                        else if (sea.Season.Season1 == "Summer")
                        {
                            lowRange = new DateTime(DateTime.Now.Year, 7, 1);
                            highRange = new DateTime(DateTime.Now.Year, 8, 31);
                        }
                        else if (sea.Season.Season1 == "Fall")
                        {
                            lowRange = new DateTime(DateTime.Now.Year, 9, 1);
                            highRange = new DateTime(DateTime.Now.Year, 11, 30);
                        }
                        if ((sea.SeasonYear >= lowRange && sea.SeasonYear <= highRange) && sea.Employee.Completed)
                        {
                            sea.Employee.Active = true;
                            db.Entry(sea.Employee).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else
                        {
                            sea.Employee.Active = false;
                            db.Entry(sea.Employee).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                }
                else
                {
                    ContractEmployee con = EMSPSSUtilities.GetContract(et.EmployeeId);
                    if (con.ContractStopDate > DateTime.Now && con.ContractStartDate < DateTime.Now)
                    {
                        con.Employee.Active = true;
                        db.Entry(con.Employee).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        con.Employee.Active = false;
                        db.Entry(con.Employee).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }

            }
        }
    }
}