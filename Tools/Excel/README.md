# Excel and NXOpen journals

Quite often we need to read excel files as part of our NXOpen journals. This section contains some options to do so.

The easiest way to interact with excel file is by the using Microsoft.Office.Interop.Excel namespace. However, this is only directly available in VB.NET.
It also requires that the machine executing the journal has Excel installed.

>NOTE: The reason that the Microsoft.Office.Interop.Excel namespace is automatically included in VB.NET and not in C# has to do with the different design decisions made by the two languages and their respective frameworks.
Visual Basic .NET (VB.NET) was designed to provide a more straightforward and beginner-friendly syntax for working with the .NET Framework. As part of this goal, VB.NET includes a feature called "implicit namespace referencing" that automatically includes some commonly used namespaces, such as Microsoft.Office.Interop.Excel, without requiring additional using statements.

## Excel and NXOpen journals in C#

C# journals cannot make use of the Microsoft.Office.Interop.Excel namespace. The reason for this is that the namespace is not loaded within the NXOpen environment this journal is executed in. To solution is to compile the code, which allows you to include additional references.
>NOTE: For compiling NXOpen code you need an additional license.

Luckily there are some workarounds for reading excel files in journals:
  - Use VB.NET scripts
  - Create NXOpen code in Excel (see separate section)
  - Export Excel data as csv/txt and read like any other file.
  - Using System.Data.Oledb. This is an alternative method. It uses SQL syntax for manipulating the data. Some example code can be found in ReadExcelOleDB.cs
  - Using late binding and reflection of Microsoft.Office.Interop.Excel. (see separate section)

>Note: C# was designed to be a more powerful and flexible language that is better suited for advanced programming tasks. C# does not include implicit namespace referencing by default because it was deemed that such a feature could lead to confusion and could make it harder to manage complex namespace hierarchies. As a result, C# requires you to explicitly include namespaces using using statements in order to use classes and methods from those namespaces, including Microsoft.Office.Interop.Excel. This makes the code more verbose, but also more explicit and easier to read and understand, especially in larger projects with many namespaces and classes.

### Create NXOpen code in Excel

Very often data is only read from Excel to be used as paramters in NXOpen methods. So instead of reading the excel file and then using that data in NXOpen methods, one can also create the NXOpen code in Excel by string contatenation and then copy that code into the journal. 

Section 8, Lecture 41 of the [SimCenter 3D basic NXOpen course (C#)](https://www.udemy.com/course/simcenter3d-basic-nxopen-course/?referralCode=4ABC27CFD7D2C57D220B%20) shows this approach in more detail.


### Use late binding and reflection

Using late binding and reflection, one can load a .dll at runtime and use the code in there. By loading Microsoft.Office.Interop.Excel.dll at runtime, we can access the code even though it is not loaded in the NXOpen runtime environment. All functionality from Microsoft.Office.Interop.Excel is available.

There is a downside off course. Invoking the code is not that straight forward and intellisense does not work. ReadExcelReflection.cs contains some example code on how to read data from an Excel file. A class NXOpenExcel has been added to simplify the code. The same example, without using the NXOpenExcel class is also added as commented code for reference.

## Executing Excel and NXOpen journal on non-Windows machines

If journals need to execute on non-Windows machines, Microsoft.Office.Interop.Excel is no longer an option since it requires an Excel installation. For these cases one can use [EPPlus](https://www.epplussoftware.com/). Versions 5 and above require a license, but older versions are free to use. For journals one can again use late binding.

Switching to Python can also be an option. Then you can use openpyxl.
>NOTE: Openpyxl can only read and write Excel FILES, it will not recalculate after changes.