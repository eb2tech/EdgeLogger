using CommunityToolkit.Mvvm.Input;
using EdgeLogger.Mobile.Models;

namespace EdgeLogger.Mobile.PageModels;

public interface IProjectTaskPageModel
{
	IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
	bool IsBusy { get; }
}