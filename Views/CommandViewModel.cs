using System;
using System.Windows.Input;

namespace WPFPages
{
        public class CommandViewModel: ViewModelBase
        {
                public CommandViewModel (string displayName, ICommand command)
                {
                        if (command == null)
                                throw new ArgumentNullException ("command");
#pragma MVVM TODO

                        //                        base.DisplayName = displayName;
                        this.Command = command;
                }
                public ICommand Command { get; private set; }
        }
}
