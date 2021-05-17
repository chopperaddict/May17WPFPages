using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;

namespace WPFPages
{
	public static class DataGridBehaviour
	{
          /// <summary>
          /// Extends <see cref="DataGrid"/> element functionality.
          /// NOT USED RIGHT NOW, but see Bank DATAGRID
          /// </summary>
          /// 
          #region - Dependency properties -

          /// <summary>
          /// Forces row selection on empty cell, full row select.
          /// </summary>
          public static readonly DependencyProperty FullRowSelectProperty = DependencyProperty.RegisterAttached("FullRowSelect",
              typeof(bool),
              typeof(DataGridBehaviour),
              new UIPropertyMetadata(false, OnFullRowSelectChanged));

          #endregion

          #region - Public methods -

          /// <summary>
          /// Gets property value.
          /// </summary>
          /// <param name="grid">Frame.</param>
          /// <returns>True if row should be selected when clicked outside of the last cell, otherwise false.</returns>
          public static bool GetFullRowSelect(DataGrid grid) {
               return (bool)grid.GetValue(FullRowSelectProperty);
          }

          /// <summary>
          /// Sets property value.
          /// </summary>
          /// <param name="grid">Frame.</param>
          /// <param name="value">Value indicating whether row should be selected when clicked outside of the last cell.</param>
          public static void SetFullRowSelect(DataGrid grid, bool value) {
               grid.SetValue(FullRowSelectProperty, value);
          }

          #endregion

          #region - Private methods -

          /// <summary>
          /// Occurs when FullRowSelectProperty has changed.
          /// </summary>
          /// <param name="depObj">Dependency object.</param>
          /// <param name="e">Event arguments.</param>
          private static void OnFullRowSelectChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e) {
               DataGrid grid = depObj as DataGrid;
               if (grid == null)
                    return;

               if (e.NewValue is bool == false) {
                    grid.MouseDown -= OnMouseDown;

                    return;
               }

               if ((bool)e.NewValue) {
                    grid.SelectionMode = DataGridSelectionMode.Single;

                    grid.MouseDown += OnMouseDown;
               }
          }

          private static void OnMouseDown(object sender, MouseButtonEventArgs e) {
                        return;

               var dependencyObject = (DependencyObject)e.OriginalSource;

               while ((dependencyObject != null) && !(dependencyObject is DataGridRow)) {
                    dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
               }

               var row = dependencyObject as DataGridRow;
               if (row == null) {
                    return;
               }
               try {
                    row.IsSelected = true;
			}
			catch {
                    return;
			}
          }

          #endregion
     }

}
