# PowerShell Commands

This file lists some of the powershell commands we use and how to use them. Examples are base off of actual code we use to give a better understanding of what they are doing.

* [Common Parameters](https://ss64.com/ps/common.html)
* [ErrorVariable & ErrorAction](https://blogs.technet.microsoft.com/heyscriptingguy/2014/07/09/handling-errors-the-powershell-way/)

## Get-AzureRmResourceGroup
[Documentation](https://docs.microsoft.com/en-us/powershell/module/azurerm.resources/get-azurermresourcegroup?view=azurermps-5.1.1)

Example
```powershell
Get-AzureRmResourceGroup -Name $ResourceGroupName -ev createNewResourceGroup -ea 0
```
| Parameter|Description|
|-|-|
| Name| The name of the resource group you are looking for |
| ev| ErrorVariable - If the command fails it will set createNewResourceGroup to the errors 
| ea| ErrorAction - what should happen if there is an error |

ErrorAction can be SilentlyContinue, Stop, Continue or Inquire. But as [this](https://blogs.msdn.microsoft.com/powershell/2008/03/29/erroraction-silentlycontinue-gt-ea-0/) blog post points out, it's just an enum under the covers so `-ea 0` is the same as `-ea SilentlyContinue` but reads better.