The development and the execution of the plug-in, was performed in a processor Intel Core i7-7800X CPU @ 3.50GHz and 16GB RAM.
The OS was the Windows 10 Pro, version 22H2, while the software used are Matlab2018b, Rhino V6.5 and Visual Studio 2019.

To run the plug-in, the 'VPL_DTO.gha' and 'VPL_DTO_mat.dll' files must be placed in the folder 
Libraries of the Grasshopper installation folder. Find the above files in the folder 'gha_dll'.
In case of using different Matlab or Rhino version it's mandatory to reproduce the files.

To generate the 'VPL_DTO_mat.dll' file, the user must compile the function 'VPL_DTO_mat.m'.
In the folder 'PolyDyna_Files' the .m files from PolyDyna code must be placed. 
The user can find these files from the following publication:
"O Giraldo-Londoρo, GH Paulino, "PolyDyna: A Matlab implementation for topology optimization 
of structures subjected to dynamic loads", Structural and Multidisciplinary Optimization, 2021
DOI http://dx.doi.org/10.1007/s00158-021-02859-6."
In the compiling procedure the user must be careful to give the same name for the namespace ('VPL_DTO_mat') 
and the class ('VPL_DTO_matClass') with the ones that are used in the 'GhcDynaTop.cs' file.
Use the Library Compiler from MATLAB Compiler SDK, with the Type .NET Assembly.
For more information about how to compile a MATLAB function to .dll check the following documantation:
'https://www.mathworks.com/products/matlab-compiler-sdk.html'

To generate the 'VPL_DTO.gha', the user must rebuild the C# project in the folder 'VPL_DTO'.
The 'VPL_DTO.csproj' have two extra references, the 'MWArray' and the 'VPL_DTO_mat.dll' 
generated above.

MATLAB
The function 'VPL_DTO_mat' gathers all the geometrical, FEM and optimization properties from the 
'GhcDynaTop.cs' and tranform them in a suitable format, in order to run the PolyDyna function.

C#
The 'GhcDynaTop.cs' file gathers all the geometrical, FEM and optimization properties assigned from
the user in the Grasshopper environment and tranform them in a format to pass in the MATLAB file.
The 'GhcFEMProperties.cs' gathers the FEM properties.
The 'GhcOPTProperties.cs' gathers the Optimization properties.
The 'GhcFinalStruct.cs' gathers the final densities of the optimized structures and visualize the 
final structure.

