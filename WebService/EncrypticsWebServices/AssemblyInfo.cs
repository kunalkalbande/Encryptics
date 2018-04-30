using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Permissions;

//
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly: AssemblyTitle("")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("")]
[assembly: AssemblyCopyright("")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]		

//
// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:

[assembly: AssemblyVersion("0.9.0.0")]

//
// In order to sign your assembly you must specify a key to use. Refer to the 
// Microsoft .NET Framework documentation for more information on assembly signing.
//
// Use the attributes below to control which key is used for signing. 
//
// Notes: 
//   (*) If no key is specified, the assembly is not signed.
//   (*) KeyName refers to a key that has been installed in the Crypto Service
//       Provider (CSP) on your machine. KeyFile refers to a file which contains
//       a key.
//   (*) If the KeyFile and the KeyName values are both specified, the 
//       following processing occurs:
//       (1) If the KeyName can be found in the CSP, that key is used.
//       (2) If the KeyName does not exist and the KeyFile does exist, the key 
//           in the KeyFile is installed into the CSP and used.
//   (*) In order to create a KeyFile, you can use the sn.exe (Strong Name) utility.
//        When specifying the KeyFile, the location of the KeyFile should be
//        relative to the "project output directory". The location of the project output
//        directory is dependent on whether you are working with a local or web project.
//        For local projects, the project output directory is defined as
//       <Project Directory>\obj\<Configuration>. For example, if your KeyFile is
//       located in the project directory, you would specify the AssemblyKeyFile 
//       attribute as [assembly: AssemblyKeyFile("..\\..\\mykey.snk")]
//        For web projects, the project output directory is defined as
//       %HOMEPATH%\VSWebCache\<Machine Name>\<Project Directory>\obj\<Configuration>.
//   (*) Delay Signing is an advanced option - see the Microsoft .NET Framework
//       documentation for more information on this.
//
//[assembly: AssemblyDelaySign(true)]
//[assembly: AssemblyKeyFile("")]
//[assembly: AssemblyKeyName("TaceoKeyPair")]

// Taceo Public Key :
// 002400000480000094000000060200000024000052534131000400000100010015817BBF8613296F
// 0E00A5485F836B4396A2A21195D2C9BA14CE651086D1B074430497D26CD967D0E5974ED1F1535720
// 59394E80B2F2A6A8AA49C4D55BC7C1FA8D1BE994A0405F7D0420563E0BEF9F403B15F6409E34AAAD
// B35EE87CC4B4EFC4F1FCF33DA77DAFC1DBD84CF273B6EAFBEBEFE6993060706126B76D53EB7758C5

[assembly: StrongNameIdentityPermissionAttribute( SecurityAction.RequestMinimum, 
PublicKey="00240000048000009400000006020000002400005253413100040000010001001" +
"5817BBF8613296F0E00A5485F836B4396A2A21195D2C9BA14CE651086D1B074430497D26CD9" +
"67D0E5974ED1F153572059394E80B2F2A6A8AA49C4D55BC7C1FA8D1BE994A0405F7D0420563" +
"E0BEF9F403B15F6409E34AAADB35EE87CC4B4EFC4F1FCF33DA77DAFC1DBD84CF273B6EAFBEB" +
"EFE6993060706126B76D53EB7758C5" )]
