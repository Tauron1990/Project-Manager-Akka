using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Tauron.Application.CommonUI.Dialogs;

[PublicAPI]
public interface IBaseDialog<TData, in TViewData>
{
    Task<TData> Init(TViewData initalData);
}