using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlEditor
{
    internal class UpdateDatabaseNotes
    {
        /*
First, make the following changes using SSMS  (sql files found in Documents > SQL Server management Studio)
1.  Drop constraints - this drops all the "non-zero" constraints

2.  Change Column names - changes column names to English - we have sql file for this

3.  Change Table names - changes tables name to English - we have sql file for this

4.  Drop 3 "answer - question" tables  (? if sql file given)

5.  Do the following (no sql file given)
    a.  Drop "non-student" table
    b.  Add All Requirement tables - You can generate this code in SQL Server Management Studio
    c.  Add new Departments table - Add values for short names:  NT, OT, PT, TH, CH, BC, --
    d.  Drop Primary keys that have chinese names and then re-add (or just change the name)
   'Backup database at every drop or change content step'

Next Create all ForeignKeys and  Display Keys - important to do to prevent deleting things you should not delete
Next Create all ForeignKeys and  Display Keys.  Use tools

1.  Under tools, you can check if a proposed non-Foreignkey can become a foreign key.

2.  Under tools & context menu, you can check if proposed "Display key" list is unique - (manually check is_DK for proposed DK list)

3.  For duplicates in CourseTerms, add a column called "section".
    THEN, CHECK 'CREDITS' and "FACULTY" (as well as course, term, section) as DK and merge these; 
    FINALLY, divide things with different faculty or credits into sections using the following SQL

    WITH CTE AS
    (Select CourseTerms.courseID, CourseTerms.section as sec, ROW_NUMBER() 
    OVER  (PARTITION BY CourseTerms.courseID, CourseTerms.termID Order by CourseTerms.CourseTermID) RN  From CourseTerms)

    UPDATE CTE
    SET CTE.sec = RN

    GO

4.  There is also a table option that allows you to edit display keys - which can eliminate a duplicate


Other things to do
5.  Replace the Departs_Old table with Requirements table  (and at the very end delete Departs_Old Table 
    a. Add new DepartmentID column to Courses and give short name of deparment for each course.
       To do this run following script in Sql Sever Managment studio, and then set this as FK and DK.
       (Assumes we have created new Departments Table - simple table with 7 rows as described above) 

        UPDATE [dbo].[Courses]
            SET [deparmentID] = [Departments].DepartmentID
            FROM COURSES Inner Join Departs_Old on Courses.depart_oldID = Departs_Old.depart_oldID
				          Inner Join Departments on Departs_Old.departmentName = Departments.Dep_ShortName
            WHERE Departments.Dep_ShortName = Departs_Old.departmentName
        GO

    b. Transfer old requirements from Departs_Old to new requirement table - (eventually will delete old_ReqID column)
        (The Departs_Old is really a requirements table.)
        INSERT INTO Requirements
			([req_name]
           ,[e_req_name]
           ,[degreeLevelID]
		   ,[old_ReqID] )
        SELECT 
			Concat_WS('-',departmentName,requiredCourse),
			Concat_WS('-',departmentName,eRequiredCourse),
			'2',
			depart_oldID
        FROM [dbo].[Departs_Old]

    c. Copy these rows into Requirements again, but with 3 as degreeLevelID - i.e. graduate
        INSERT INTO [dbo].[Requirements]
           ([req_name]
           ,[e_req_name]
           ,[degreeLevelID]
           ,[old_ReqID])
        Select 
           [Requirements].[req_name]
           ,[Requirements].[e_req_name]
           ,'3'
           ,[old_ReqID]
        FROM
	        Requirements

    d. Add New RequirementID column to Grad_Requirement, and make it "match" the depart_OldID requirement

        UPDATE [dbo].[GradRequirements]  
        SET GradRequirements.requirementID = 
        (
            Select Requirements.requirementID From Requirements
	        inner join Departs_Old on Requirements.old_ReqID = Departs_Old.depart_oldID
            Where GradRequirements.departmentID = Departs_Old.depart_oldID
	    ) 
    
        (Transfer credit table should be done here with similar logic for departments - see below step p.)

    e.  Add degreeLevelID as FK to COURSETERMS, DEGREES)  (Set these = old 'graduateCourse' 'graduateDegree'.) 

        UPDATE [dbo].[CourseTerms]
        SET CourseTerms.DegreeLevelID =
        CASE WHEN  CourseTerms.graduateCourse = 'True' THEN 2   -- CHANGE TO MASTER LEVEL
        Else 3 END   -- CHANGE TO UNDERGRAD LEVEL

        UPDATE [dbo].[Degrees]
        SET Degrees.degreeLevelID =
        CASE WHEN  Degrees.graduateDegree = 'True' THEN 2   -- CHANGE TO MASTER LEVEL ID
        Else 3 END   -- CHANGE TO UNDERGRAD LEVEL

    f. Update the GradRequirement requirementID based on the degree (2 or 3)

        I have two sets of requirments, the first set for Masters, the second set for undergrad.
        Set these Gradrequirements all to the first set, i.e. masters
        Now change the GradRequirements for a non-Masters degree to the second set with the following:.

        UPDATE [dbo].[GradRequirements] 
        SET GradRequirements.requirementID =  R3.requirementID
        FROM GradRequirements INNER JOIN Requirements as R2 On GradRequirements.requirementID = R2.requirementID
		    Inner Join Requirements r3 On R3.req_name = R2.req_name
		    Inner Join Degrees on GradRequirements.degreeID = Degrees.degreeID
	        Where Degrees.degreeLevelID = 3  AND NOT R3.requirementID = GradRequirements.requirementID -- R3 is the Bachelor

   'Backup database after any permanent change or a few non-permanent changes'

		
    g. Push the DegreeLevelID down from CourseTerms to Courses.
       (Use same stratagy as above.)
        g1.  Add DegreeLevelID - set default as 3
                Update to 2 if "gradudateCourse" = 'true'
            
        g2. Add degreeLevelID to courses - set default as 3

        g3. Double the number of courses with the second set seting degreeLevelID to 2

        INSERT INTO [dbo].[Courses]
           ([courseName]
           ,[degreeLevelID]
           ,[eCourseName]
           ,[deparmentID]
           ,[depart_oldID]
           ,[note]
           ,[section])
        Select
           [courseName]
           , '3'
           ,[eCourseName]
           ,[deparmentID]
           ,[depart_oldID]
           ,[note]
           ,[section]
	    FROM Courses
        GO
               
        
        g4.  Map CourseTerms.CourseID to Course with correct requirement, i.e. requirement with degreeLevel 2 for grads and requirement with 3 for BA

        -- Assume original courses are all labeled 2, i.e. grad courses, and select a course with same name and department but on level 3, i.e. BA 
        UPDATE [dbo].[CourseTerms]  
        SET CourseTerms.courseID = 
        c3.courseID
		FROM CourseTerms INNER JOIN Courses AS c2  on CourseTerms.CourseID = c2.courseID
			Inner Join Requirements  on c2.depart_oldID = Requirements.old_ReqID
			Inner Join Courses c3 on c2.courseName = c3.CourseName	    
			WHERE c3.depart_oldID = c2.depart_oldID AND CourseTerms.degreeLevelID = '3' and c3.degreeLevelID = '3'

        GO
         
    h.  Set Courses.RequirmentID from OLD ID and course Level
        (Missing Sql - but not hard)    

    i.  Delete unused FK in Requirements and in Courses  (Use ^U and Delete)

   'Backup database after any permanent change or a few non-permanent changes'


    j.  Transfer listingCredit to note and delete column

        UPDATE [dbo].[transcript]
           SET [note] = 
           CASE WHEN   transcript.listeningCredit = 'True' THEN Concat('Listen Credit ', note)
           Else transcript.note END
        GO

    k.  Added "GradeStatus"
        k1.  This is FK in transcript table - options (in statusKey column): "forCredit, auditting, dropped, withdrawn, etc.
        k2.  Use following to set the status based on the audit column of transcripts - then delete audit column.
            'Backup database at every step'

        UPDATE [dbo].[transcript]
        SET [gradeStatusID] = 2   -- Must pick "audit" gradeStatusID
        FROM transcript
        Where transcript.auditing = 'True'

    l.  Added "RequirementStatus
        l1.  This is a FK of the requirement table - the name of the requirement should reflect this status
        12.  Examples (in statusKey column):  online, onlineSimultanius, inPerson, DVD, directedStudy
            (GradRequirments could be "60 online credits", "60 onlineSimulanius or inPerson credits")
        13.  Use following to update and then delete 'self-study' column in transcripts
            'Backup database at every step'

        UPDATE [dbo].[transcript]
        SET [requirementStatusID] = 2
        FROM transcript
        Where transcript.selfStudyCourse = 'True'
        GO

        UPDATE [dbo].[transcript]
        SET [requirementStatusID] = 4
        FROM transcript
        where note Like '%DVD%'
        GO

    M. Create table "RequirementNames" and fill it with the following - 34 rows
        2023.10 - Skip Requirement Names - include name and eName in requirements
        2023.10 - RequirementMap is between Requirements
        (Could do this earlier.)
        INSERT INTO [dbo].[RequirementName]
                   ([name]
                   ,[e_name])
        Select  DISTINCT
		        Requirements.req_name,
		        Requirements.e_req_name
        FROM	Requirements
        GO

        UPDATE [dbo].[Requirements]
           SET [requirementNameID] = RequirementName.requirementNameID
           From RequirementName
         WHERE Requirements.req_name = RequirementName.name AND Requirements.e_req_name = RequirementName.e_name
        GO



    M2.  And then add RequirementNames as FK to Requirments, and then delete two columns from Requrirements



    N.  Add Handbooks table.  Insert rows with
        INSERT INTO [dbo].[Handbooks]
           ([handbook])
        Select  DISTINCT
		        GradRequirements.yearbook
        FROM	GradRequirements

        Update GradRequirements and studentDegrees with handbookID column using

        UPDATE [dbo].[GradRequirements]
        SET  [handbookID] = 
        (Select Handbooks.handbookID From Handbooks Where HandBooks.handbook = GradRequirements.yearbook)
   
        StudentDegrees yearbook has various errors, so before updating we must
        1.  Add "Unknown" as a value in new Handbooks table (probably ID 8)
            Add "1997" as a value in new Handbooks table

        2.  Use sqlEditor to change all values with leading spaces in "yearbook" - first change to 1000 then back to original value

        To find other problem rows use
        select yearbook from studentDegrees WHERE NOT EXISTS ( Select handbook FROM Handbooks Where handbook = yearbook)

        UPDATE StudentDegrees
        SET yearBook = 'Unknown' Where yearbook is null

        UPDATE StudentDegrees
        SET yearBook = 'Unknown' Where yearbook = '80'

        UPDATE StudentDegrees
        SET yearBook = '2008' Where yearbook = '2009'

        UPDATE StudentDegrees
        SET yearBook = '2008' Where yearbook = '2009'

        UPDATE StudentDegrees
        SET yearbook = '2012' Where yearbook = '2011' OR yearbook = '2013'

        Once there are no problem rows, run
        UPDATE [dbo].[StudentDegrees]
        SET  [handbookID] = 
        (Select Handbooks.handbookID From Handbooks Where HandBooks.handbook = StudentDegrees.yearbook)

    o.  Add "deliveryMethodID" to degrees - and make 3 degrees for every one
    o1.        INSERT INTO [dbo].[Degrees]
                   ([degreeName]
                   ,[degreeNameLong]
                   ,[eDegreeName]
                   ,[eDegreeNameLong]
                   ,[degreeLevelID]
                   ,[deliveryMethodID]
                   ,[note])
                   Select 
                   [Degrees].[degreeName]
                   ,[Degrees].[degreeNameLong]
                   ,[Degrees].[eDegreeName]
                   ,[Degrees].[eDegreeNameLong]
                   ,[Degrees].[degreeLevelID]
                   ,'2'
                   ,[note]
                From Degrees
                Where Degrees.deliveryMethodID = 1
                GO
    o2. Repeat the above with "2" as delivery method.
    o3. Update the names

        UPDATE [dbo].[Degrees]
        SET 
	        [degreeName] = '遠距' + degreeName
            ,[degreeNameLong] = '遠距' + degreeNameLong
            ,[eDegreeName] = 'Online ' + eDegreeName
            ,[eDegreeNameLong] = 'Online ' + eDegreeNameLong
	    WHERE Degrees.deliveryMethodID = 2 OR Degrees.deliveryMethodID = 3
        GO

    o4. Ask school to switch studentdegree to these for all online students 

    p.  Transfer credits
        1.  Delete "FulfillsGradRequirements from Grades table
        2.  Transfer table: 
            a. Add requirementID.  Run (this grabs the first because we have already added 2 - 1 for Master and 1 for BA

                UPDATE [dbo].[TransferCredits]
                SET 
                  [requirementID] = 
		            (
			            Select Top 1 Requirements.requirementID From Requirements
			            inner join Departs_Old on Requirements.depart_OldID = Departs_Old.depart_oldID
			            Where TransferCredits.requirementID = Departs_Old.depart_oldID
		            ) 

                GO


               Correct FK errors in SqlEditor - and make it a foreign key
            b. Add deliveryMethod - set default to "1", and make a foreign key
            c. FullfillsRequirmentNoCredits column - bit (obvious meaning) 
            d. Delete the S grade in grades file after chaning to P it in all transcripts 




    z.  Finally, delete all old, unused columns.       DO THIS AFTER WEEKS OF TESTING
         
         
         
         
         
         
         
         
         
         
         
         */


    }
}

/// OLD Student Requirement table fill - works but too slow
////public static void fillGradRequirementsDT(int studentDegreeID, ref StringBuilder sbErrors)
////{
////    // 1. Create a table "StudentReq" - this table is not in the database.
////    // Add this table to dataHelper.fieldsDT so that I can use an sqlFactory to style the table 
////    // Add rows to dataHelper.fieldsDT. Only do it once in a session
////    string filter = "TableName = 'StudentReq'";
////    DataRow[] drs = dataHelper.fieldsDT.Select(filter);
////    // If no rows in above, the rows have already been added to dataHelper.fieldsDT.  14 columns as follows:
////    // public static void AddRowToFieldsDT(string TableName, int ColNum, string ColumnName, string ColumnDisplayName
////    //      , string DataType, bool Nullable, bool _identity, bool is_PK, bool is_FK, bool is_DK, short MaxLength
////    //      , string RefTable, string RefPkColumn, int Width)
////    if (drs.Count() == 0)
////    {
////        dataHelper.AddRowToFieldsDT("StudentReq", 1, "StudentReqID", "StudentReqID", "int", false, true, true, false, false, 4, String.Empty, String.Empty, 0);
////        //RequirementType table -
////        dataHelper.AddRowToFieldsDT("StudentReq", 2, "ReqType", "ReqType", "nvarchar", false, false, false, false, true, 200, String.Empty, String.Empty, 0);
////        dataHelper.AddRowToFieldsDT("StudentReq", 10, "eReqType", "eReqType", "nvarchar", false, false, false, false, true, 200, String.Empty, String.Empty, 0);
////        //RequirementArea table - 
////        dataHelper.AddRowToFieldsDT("StudentReq", 3, "ReqArea", "ReqArea", "nvarchar", false, false, false, false, true, 200, String.Empty, String.Empty, 0);
////        dataHelper.AddRowToFieldsDT("StudentReq", 12, "eReqArea", "eReqArea", "nvarchar", false, false, false, false, true, 200, String.Empty, String.Empty, 0);
////        // DeliveryMethodTable
////        dataHelper.AddRowToFieldsDT("StudentReq", 4, "DelMethName", "DelMethName", "nvarchar", false, false, false, false, true, 200, String.Empty, String.Empty, 0);
////        dataHelper.AddRowToFieldsDT("StudentReq", 14, "eDelMethName", "eDelMethName", "nvarchar", false, false, false, false, true, 200, String.Empty, String.Empty, 0);
////        dataHelper.AddRowToFieldsDT("StudentReq", 15, "rDeliveryLevel", "rDeliveryLevel", "int", false, false, false, false, true, 4, String.Empty, String.Empty, 0);
////        // GradRequirement Table itself
////        dataHelper.AddRowToFieldsDT("StudentReq", 5, "Required", "Required", "real", false, false, false, false, false, 4, String.Empty, String.Empty, 0);
////        dataHelper.AddRowToFieldsDT("StudentReq", 16, "Limit", "Limit", "real", false, false, false, false, false, 4, String.Empty, String.Empty, 0);
////        // Need to calucate the following from transcript
////        dataHelper.AddRowToFieldsDT("StudentReq", 6, "Earned", "Earned", "real", false, false, false, false, false, 4, String.Empty, String.Empty, 0);
////        dataHelper.AddRowToFieldsDT("StudentReq", 7, "Needed", "Needed", "real", false, false, false, false, false, 4, String.Empty, String.Empty, 0);
////        dataHelper.AddRowToFieldsDT("StudentReq", 8, "InProgress", "InProgress", "real", false, false, false, false, false, 4, String.Empty, String.Empty, 0);
////        dataHelper.AddRowToFieldsDT("StudentReq", 17, "Icredits", "Icredits", "real", false, false, false, false, false, 4, String.Empty, String.Empty, 0);
////    }

////    // 2.  Get the table
////    StringBuilder sb = MsSql.getFillStudentRequirementTableSql(studentDegreeID);
////    DataTable reqDataTable = new DataTable();
////    String sqlString = sb.ToString();
////    MsSql.FillDataTable(reqDataTable, sqlString);

////    //// 2. Get Grad Requirements for this degree and handbook
////    //int intHandbookID = Int32.Parse(dataHelper.getColumnValueinDR(PrintToWord.studentDegreeInfoDT.Rows[0], "handbookID"));
////    //int intDegreeID = Int32.Parse(dataHelper.getColumnValueinDR(PrintToWord.studentDegreeInfoDT.Rows[0], "degreeID"));
////    //SqlFactory sqlGradReq = new SqlFactory(TableName.gradRequirements, 0, 0);
////    //where wh1 = new where(TableField.GradRequirements_DegreeID, intDegreeID.ToString());
////    //where wh2 = new where(TableField.GradRequirements_handbookID, intHandbookID.ToString());
////    //sqlGradReq.myWheres.Add(wh1);
////    //sqlGradReq.myWheres.Add(wh2);
////    //string sqlString = sqlGradReq.returnSql(command.selectAll);
////    //// Put degree requirements in a new DataTable grDaDt.dt
////    //MsSqlWithDaDt grDaDt = new MsSqlWithDaDt(sqlString);

////    //// 3. Put all the basic information about this degree, degreeLevel, degreeDeliveryMethod into 3 dictionaries.
////    ////    Use "GetPkRowColumnValues" to create the dictionary.
////    ////    Use the dictionary to get any value you want about this degree.  For example, sDegreeLevelColValue["degreeLevel"]
////    //// Degree - "degreeName", "deliveryMethodID", "eDegreeName", "degreeLevelID"
////    //List<string> sDegreeColNames = new List<string> { "degreeName", "eDegreeName", "degreeLevelID" };
////    //Dictionary<string, string> sDegreeColValues = TranscriptHelper.GetPkRowColumnValues(
////    //        TableName.degrees, intDegreeID, sDegreeColNames, ref sbErrors);

////    //// degreeLevel - "degreeLevelName", "degreeLevel"
////    //int sDegreeLevelID = Int32.Parse(sDegreeColValues["degreeLevelID"]);
////    //List<string> sDegreeLevelColNames = new List<string> { "degreeLevelName", "degreeLevel" };
////    //Dictionary<string, string> sDegreeLevelColValues = TranscriptHelper.GetPkRowColumnValues(
////    //        TableName.degreeLevel, sDegreeLevelID, sDegreeLevelColNames, ref sbErrors);
////    //int sDegreeLevel = Int32.Parse(sDegreeLevelColValues["degreeLevel"]);
////    //string sDegreeLevelName = sDegreeLevelColValues["degreeLevelName"];

////    // 4. Create sqlFactory for studentReq - this will give you sqlStudentReq.myFields()
////    SqlFactory sqlStudentReq = new SqlFactory("StudentReq", 0, 0);

////    // 5. Create studentReqDT and add a column for each field.
////    //    We will add a row to this table for each requirement and fill it. 
////    PrintToWord.studentReqDT = new System.Data.DataTable();
////    foreach (field f in sqlStudentReq.myFields)
////    {
////        DataColumn dc = new DataColumn(f.fieldName, dataHelper.ConvertDbTypeToType(f.dbType));
////        // Make StudentReqID the primary key
////        if (f.fieldName == "StudentReqID")
////        {
////            dc.AutoIncrement = true;
////            dc.AutoIncrementSeed = 1;
////            dc.AutoIncrementStep = 1;
////        }
////        PrintToWord.studentReqDT.Columns.Add(dc);
////    }

////    //------------------------------------------------------------------------------------//
////    // Outer loop - graduation requirements
////    //------------------------------------------------------------------------------------//

////    //decimal totalQpaCredits = 0;  // Only update on the first inner loop through transcripts
////    //decimal totalQpaPoints = 0;
////    //decimal totalCreditsEarned = 0;
////    //bool firstLoop = true;
////    //// 6. Main routine: Fill studentReqDT 
////    //// Old degrees might not have any requirements, but I need one to get the QPA.
////    //// So I get the first row in the gradReq table along with a 'note' that this is a fakeRow
////    //bool fakeRow = false;
////    //if (grDaDt.dt.Rows.Count == 0)
////    //{
////    //    // These values are not used, so I just use the first row in the gradRequirement Table.
////    //    sqlGradReq.myWheres.Clear();
////    //    sqlString = sqlGradReq.returnSql(command.selectAll);
////    //    grDaDt = new MsSqlWithDaDt(sqlString);
////    //    fakeRow = true;
////    //}

////    //foreach (DataRow drGradReq in grDaDt.dt.Rows)
////    //{
////    //    //6a. Get information we need from drGradReq - use "r" for requirement.
////    //    Decimal rCreditLimit = Decimal.Parse(dataHelper.getColumnValueinDR(drGradReq, "creditLimit"));
////    //    Decimal rReqCredits = Decimal.Parse(dataHelper.getColumnValueinDR(drGradReq, "reqUnits"));
////    //    int rGradReqTypeID = Int32.Parse(dataHelper.getColumnValueinDR(drGradReq, "gradReqTypeID"));
////    //    int rRequirementAreaID = Int32.Parse(dataHelper.getColumnValueinDR(drGradReq, "reqAreaID"));
////    //    int rDeliveryMethodID = Int32.Parse(dataHelper.getColumnValueinDR(drGradReq, "rDeliveryMethodID"));

////    //    //6b. requirementAreaID - Get information from gradRequirmentType Table
////    //    List<string> rReqAreaTableColNames = new List<string> { "reqArea", "eReqArea" };
////    //    Dictionary<string, string> rReqAreaTableColValues = TranscriptHelper.GetPkRowColumnValues(
////    //        TableName.requirementArea, rRequirementAreaID, rReqAreaTableColNames, ref sbErrors);
////    //    string rReqArea = rReqAreaTableColValues["reqArea"];
////    //    string rReqAreaEng = rReqAreaTableColValues["eReqArea"];

////    //    //6c. reqTypeID - Get information from requirementType table
////    //    List<string> rGradTypeColNames = new List<string> { "reqTypeDK"};
////    //    Dictionary<string, string> rGradTypeColValues = TranscriptHelper.GetPkRowColumnValues(
////    //        TableName.gradRequirementType, rGradReqTypeID, rGradTypeColNames, ref sbErrors);
////    //    string rGradReqTypeName = rGradTypeColValues["reqTypeDK"];

////    //    //6d. deliveryMethodID - Get the requirement deliveryMethod table
////    //    List<string> rDeliveryMethodColNames = new List<string> { "delMethName", "eDelMethName", "deliveryLevel" };
////    //    Dictionary<string, string> rDeliveryMethodColValues = TranscriptHelper.GetPkRowColumnValues(
////    //        TableName.deliveryMethod, rDeliveryMethodID, rDeliveryMethodColNames, ref sbErrors);
////    //    string rDelMethName = rDeliveryMethodColValues["delMethName"];
////    //    string rDelMethNameEng = rDeliveryMethodColValues["eDelMethName"];
////    //    int rDeliveryLevel = Int32.Parse(rDeliveryMethodColValues["deliveryLevel"]);
////    //    // For level zero, don't give user the name; just leave it empty
////    //    if (rDeliveryLevel == 0)
////    //    {
////    //        rDelMethName = string.Empty; ;
////    //        rDelMethNameEng = string.Empty; ;
////    //    }

////    //6e. Create a row for studentReqDT, and partially fill it with drGradReq info. extracted above.
////    //   (Except for credits earned / inProgress / needed, we have all that we need to fill this row )

////    DataRow dr = PrintToWord.studentReqDT.NewRow();

////    // "StudentReqID" is the Primary key and column set to autoincrement.
////    dataHelper.setColumnValueInDR(dr, "Required", rReqCredits);
////    dataHelper.setColumnValueInDR(dr, "Limit", rCreditLimit);
////    dataHelper.setColumnValueInDR(dr, "ReqArea", rReqArea);
////    dataHelper.setColumnValueInDR(dr, "eReqArea", rReqAreaEng);

////    dataHelper.setColumnValueInDR(dr, "DelMethName", rDelMethName);
////    dataHelper.setColumnValueInDR(dr, "eDelMethName", rDelMethNameEng);
////    dataHelper.setColumnValueInDR(dr, "rDeliveryLevel", rDeliveryLevel);

////    dataHelper.setColumnValueInDR(dr, "ReqType", rGradReqTypeName);
////    dataHelper.setColumnValueInDR(dr, "Earned", 0);
////    dataHelper.setColumnValueInDR(dr, "InProgress", 0);
////    dataHelper.setColumnValueInDR(dr, "Needed", 0);
////    dataHelper.setColumnValueInDR(dr, "Icredits", 0);

////    ////------------------------------------------------------------------------------------//
////    ////   Inner Loop - Transcript rows
////    ////------------------------------------------------------------------------------------//

////    //// 7. Loop through the transcript rows - updating "earned", "InProgress", Icredits"
////    ////    Also keep track of QPA credits and QPA pointsEarned
////    ////    After loop is complete, fill in the "Needed" (= required minus earned) 
////    //decimal qpaCredits = 0;
////    //decimal qpaPoints = 0;
////    //decimal earned = 0;
////    //decimal inProgress = 0;
////    //decimal iCredits = 0;

////    //foreach (DataRow drTrans in PrintToWord.transcriptDT.Rows)
////    //{
////    //    // Get all required information about the transcript row course - add "c" before variable for "course"

////    //    // Information from transcript datarow itself - a lot
////    //    int cGradeID = Int32.Parse(dataHelper.getColumnValueinDR(drTrans, "gradeID"));
////    //    int cGradeStatusID = Int32.Parse(dataHelper.getColumnValueinDR(drTrans, "gradeStatusID"));
////    //    int cCourseTermSectionID = Int32.Parse(dataHelper.getColumnValueinDR(drTrans, "courseTermSectionID"));
////    //    int cCourseTermID = Int32.Parse(dataHelper.getColumnValueinDR(drTrans, "courseTermID"));
////    //    int cCourseID = Int32.Parse(dataHelper.getColumnValueinDR(drTrans, "courseID"));
////    //    int cCourseNameID = Int32.Parse(dataHelper.getColumnValueinDR(drTrans, "courseNameID"));
////    //    int cSectionID = Int32.Parse(dataHelper.getColumnValueinDR(drTrans, "sectionID"));
////    //    int cDegreeLevelID = Int32.Parse(dataHelper.getColumnValueinDR(drTrans, "degreeLevelID"));
////    //    int cDeliveryMethodID = Int32.Parse(dataHelper.getColumnValueinDR(drTrans, "deliveryMethodID"));
////    //    int cRequirementAreaID = Int32.Parse(dataHelper.getColumnValueinDR(drTrans, "requirementAreaID"));


////    //    // Information from Grades table
////    //    List<string> cGradesColNames = new List<string> { "grade", "QP", "earnedCredits", "creditsInQPA" };
////    //    Dictionary<string, string> cGradesColValues = TranscriptHelper.GetPkRowColumnValues(
////    //        TableName.grades, cGradeID, cGradesColNames, ref sbErrors);
////    //    string cGrade = cGradesColValues["grade"];
////    //    Decimal cGradeQP = Decimal.Parse(cGradesColValues["QP"]);
////    //    Boolean cEarnedCredits = Boolean.Parse(cGradesColValues["earnedCredits"]);
////    //    Boolean cCreditsInQPA = Boolean.Parse(cGradesColValues["creditsInQPA"]);

////    //    // Information from GradeStatus table
////    //    List<string> cGradeStatusColNames = new List<string> { "statusKey", "statusName", "eStatusName", "forCredit" };
////    //    Dictionary<string, string> cGradesStatusColValues = TranscriptHelper.GetPkRowColumnValues(
////    //        TableName.gradeStatus, cGradeStatusID, cGradeStatusColNames, ref sbErrors);
////    //    string cStatusKey = cGradesStatusColValues["statusKey"];  // Used in error msg
////    //    bool forCredit = bool.Parse(cGradesStatusColValues["forCredit"]);

////    //    // Information from Section table
////    //    List<string> cSectionColNames = new List<string> { "credits" };
////    //    Dictionary<string, string> cSectionColValues = TranscriptHelper.GetPkRowColumnValues(
////    //        TableName.section, cSectionID, cSectionColNames, ref sbErrors);
////    //    Decimal cCredits = Decimal.Parse(cSectionColValues["credits"]);

////    //    // CourseName - 
////    //    // int cCourseID = Int32.Parse(cCourseTermColValues["courseID"]);
////    //    List<string> cCourseNamesColNames = new List<string> { "courseName", "eCourseName" };
////    //    Dictionary<string, string> cCourseNamesColValues = TranscriptHelper.GetPkRowColumnValues(
////    //        TableName.courseNames, cCourseNameID, cCourseNamesColNames, ref sbErrors);
////    //    string cCourseName = cCourseNamesColValues["courseName"];  // Used in error messages only, i.e. doesn't affect requirements

////    //    // DegreeLevel - "degreeLevelName", "degreeLevel"
////    //    List<string> cDegreeLevelColNames = new List<string> { "degreeLevelName", "degreeLevel" };
////    //    Dictionary<string, string> cDegreeLevelColValues = TranscriptHelper.GetPkRowColumnValues(
////    //        TableName.degreeLevel, cDegreeLevelID, cDegreeLevelColNames, ref sbErrors);
////    //    int cDegreeLevel = Int32.Parse(cDegreeLevelColValues["degreeLevel"]);
////    //    string cDegreeLevelName = cDegreeLevelColValues["degreeLevelName"];  // Used in error msg

////    //    // deliveryMethod - "delMethName", "eDelMethName", "deliveryLevel"
////    //    List<string> cDelMethColNames = new List<string> { "delMethName", "eDelMethName", "deliveryLevel" };
////    //    Dictionary<string, string> cDelMethColValues = TranscriptHelper.GetPkRowColumnValues(
////    //        TableName.deliveryMethod, cDeliveryMethodID, cDelMethColNames, ref sbErrors);
////    //    int cDeliveryLevel = Int32.Parse(cDelMethColValues["deliveryLevel"]);

////    //    // Requirements - "reqArea", "eReqArea"
////    //    List<string> cReqColNames = new List<string> { "reqArea", "eReqArea", "Ancestors" };
////    //    Dictionary<string, string> cReqColValues = TranscriptHelper.GetPkRowColumnValues(
////    //        TableName.requirementArea, cRequirementAreaID, cReqColNames, ref sbErrors);
////    //    string cReqArea = cReqColValues["reqArea"];
////    //    string cAncestors = cReqColValues["Ancestors"];

////    //    //Error message
////    //    if (cEarnedCredits && !forCredit && !fakeRow)
////    //    {
////    //        // cStatusKey is not "forCredit" but the grade indicates student has earned credit
////    //        string strWarn = String.Format("{0} has a grade but its statusKey is {1}. ", cCourseName, cStatusKey);
////    //        sbErrors.AppendLine(strWarn);
////    //    }

////    //    // Update "Earned", "InProgress", "Icredit" for this requirement row
////    //    // First check that the degreeLevel is correct
////    //    if (forCredit || fakeRow)  // This is needed for every type of requirement
////    //    {
////    //        // Error message
////    //        if (cDegreeLevel < sDegreeLevel && !fakeRow)
////    //        {
////    //            sbErrors.AppendLine(String.Format("Degree level of '{0} ({1})' is lower than student degree level ({2}). No credit granted.",
////    //                        cCourseName, cDegreeLevelName, sDegreeLevelName));
////    //        }
////    //        else
////    //        {
////    //            // Handle total QPA points and credits earned
////    //            // This is the only thing that we need in a fake row
////    //            if (cCreditsInQPA && firstLoop)
////    //            {
////    //                totalQpaPoints = totalQpaPoints + (cCredits * cGradeQP);
////    //                totalQpaCredits = totalQpaCredits + cCredits;
////    //            }
////    //            if (cEarnedCredits && firstLoop)
////    //            {
////    //                totalCreditsEarned = totalCreditsEarned + cCredits;
////    //            }
////    //            if (!fakeRow)
////    //            {
////    //                if (rGradReqTypeName == "QPA" || fakeRow)
////    //                {
////    //                    if (cCreditsInQPA)
////    //                    {
////    //                        qpaPoints = qpaPoints + (cCredits * cGradeQP);
////    //                        qpaCredits = qpaCredits + cCredits;
////    //                    }
////    //                }
////    //                else  // Credits or Hours or Times requirement
////    //                {
////    //                    // Get the number of "credits" or "units" earned
////    //                    if (rGradReqTypeName == "Times")
////    //                    {
////    //                        cCredits = 1;   // The main point of "Times"
////    //                    }  

////    //                    // No credit for this requirement if cDeliveryMethod < rDeliveryMethod
////    //                    if (cDeliveryLevel < rDeliveryLevel)
////    //                    {
////    //                        cCredits = 0;
////    //                    }

////    //                    // Check if this drTrans dataRow meets this requirement
////    //                    List<string> cAncestorsList = cAncestors.Split(",").ToList();
////    //                    if (rGradReqTypeName == "Credits" || rGradReqTypeName == "Hours" || rGradReqTypeName == "Times")
////    //                    {
////    //                        // Meets requirement
////    //                        if (rReqArea == cReqArea || cAncestorsList.Contains(rReqArea))
////    //                        {
////    //                            // Update earned and inProgress (for this requirement)
////    //                            if (cEarnedCredits)
////    //                            {
////    //                                earned = earned + cCredits;
////    //                            }
////    //                            else if (cGrade == "NG")
////    //                            {
////    //                                inProgress = inProgress + cCredits;
////    //                            }
////    //                            else if (cGrade == "I")
////    //                            {
////    //                                iCredits = inProgress + cCredits;
////    //                            }
////    //                        }
////    //                        else
////    //                        {
////    //                            cCredits = 0;
////    //                        }
////    //                    }
////    //                }
////    //            }
////    //        }
////    //    }
////    //}
////    //// Fill in the requirement row Earned, InProgress, Needed
////    //if (!fakeRow)
////    //{
////    if (rGradReqTypeName == "QPA")
////    {
////        decimal QPA = 0;
////        if (qpaCredits > 0)
////        {
////            QPA = Math.Round(qpaPoints / qpaCredits, 2);
////        }
////        dataHelper.setColumnValueInDR(dr, "Earned", QPA);
////        dataHelper.setColumnValueInDR(dr, "InProgress", 0);
////        if (QPA < rReqCredits)
////        {
////            dataHelper.setColumnValueInDR(dr, "Needed", rReqCredits - QPA);
////        }
////    }
////    else
////    {
////        // Fill in columns 
////        dataHelper.setColumnValueInDR(dr, "Earned", earned);
////        dataHelper.setColumnValueInDR(dr, "InProgress", inProgress);
////        if (rReqCredits > earned)
////        {
////            dataHelper.setColumnValueInDR(dr, "Needed", rReqCredits - earned);
////        }
////        else
////        {
////            dataHelper.setColumnValueInDR(dr, "Needed", 0);
////        }
////        dataHelper.setColumnValueInDR(dr, "Icredits", iCredits);
////    }
////    // Add this row to studentReqDT
////    PrintToWord.studentReqDT.Rows.Add(dr);
////    //        firstLoop = false;
////    //    }
////    //    if (fakeRow) { break; }
////    //}
////    // Update student QPA and total Credits - not yet committed to database
////    DataRow studentDegreeInfoRow = PrintToWord.studentDegreeInfoDT.Rows[0];
////    decimal totalQPA = 0;
////    if (totalQpaCredits > 0)
////    {
////        totalQPA = Math.Round(totalQpaPoints / totalQpaCredits, 2);
////    }
////    dataHelper.setColumnValueInDR(studentDegreeInfoRow, "creditsEarned", totalCreditsEarned);
////    dataHelper.setColumnValueInDR(studentDegreeInfoRow, "QPA", totalQPA);
////    dataHelper.setColumnValueInDR(studentDegreeInfoRow, "lastUpdated", DateTime.Now.ToShortDateString());

////    // Save these three changes down to the database - start over from scratch.
////    field pkField = dataHelper.getTablePrimaryKeyField(TableName.studentDegrees);
////    string pk = dataHelper.getColumnValueinDR(studentDegreeInfoRow, pkField.fieldName);
////    sqlString = String.Format("Select * from {0} where {1} = '{2}'", TableName.studentDegrees, pkField.fieldName, pk);
////    MsSqlWithDaDt dadt = new MsSqlWithDaDt(sqlString);
////    List<field> fieldsToUpdate = new List<field>();
////    fieldsToUpdate.Add(dataHelper.getFieldFromFieldsDT(TableName.studentDegrees, "creditsEarned"));
////    fieldsToUpdate.Add(dataHelper.getFieldFromFieldsDT(TableName.studentDegrees, "QPA"));
////    fieldsToUpdate.Add(dataHelper.getFieldFromFieldsDT(TableName.studentDegrees, "lastUpdated"));
////    MsSql.SetUpdateCommand(fieldsToUpdate, dadt.da);
////    dataHelper.setColumnValueInDR(dadt.dt.Rows[0], "creditsEarned", totalCreditsEarned);
////    dataHelper.setColumnValueInDR(dadt.dt.Rows[0], "QPA", totalQPA);
////    dataHelper.setColumnValueInDR(dadt.dt.Rows[0], "lastUpdated", DateTime.Now.ToShortDateString());
////    try { dadt.da.Update(dadt.dt); } catch { }
////}