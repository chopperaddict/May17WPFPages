using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFPages . ViewModels;

namespace WPFPages .Views
{
	public class MainWindowViewModel
	{
                // In MainWindowViewModel.cs 
                ObservableCollection<BankAccountViewModel> _workspaces;
                public ObservableCollection<BankAccountViewModel> Workspaces
                {
                        get
                        {
                                if (_workspaces == null)
                                {
                                        _workspaces = new ObservableCollection<BankAccountViewModel> ();
                                        _workspaces.CollectionChanged += this.OnWorkspacesChanged;
                                }
                                return _workspaces;
                        }
                }
                void OnWorkspacesChanged (object sender, NotifyCollectionChangedEventArgs e)
                {
#pragma MVVM TODO
                        //        if (e.NewItems != null && e.NewItems.Count != 0)
                        //                foreach (BankAccountViewModel workspace in e.NewItems)
                        //                        workspace.RequestClose += this.OnWorkspaceRequestClose;
                        //        if (e.OldItems != null && e.OldItems.Count != 0)
                        //                foreach (BankAccountViewModel workspace in e.OldItems)
                        //                        workspace.RequestClose -= this.OnWorkspaceRequestClose;
                }
                void OnWorkspaceRequestClose (object sender, EventArgs e)
                {
                        this.Workspaces.Remove (sender as BankAccountViewModel);
                }
        }
}
