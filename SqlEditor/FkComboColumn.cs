﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SqlEditor
{
    public class FkComboColumn : DataGridViewColumn
    {
        public FkComboColumn() : base(new FkComboCell())  {   }
       
        public override DataGridViewCell CellTemplate
        {  
            get
            {
                return base.CellTemplate;
            }
            set
            {
                // Ensure that the cell used for the template is a FkComboCell.
                if (value != null &&
                    !value.GetType().IsAssignableFrom(typeof(FkComboCell)))
                {
                    throw new InvalidCastException("Must be a FkComboCell");
                }
                base.CellTemplate = value;
            }
        }

    }

    public class FkComboCell : DataGridViewTextBoxCell
    {

        public FkComboCell() : base()
        {
            // Format the cell (not the editing control) here.  Example
            // this.Style.ForeColor = Color.Purple;
        }

        public DataTable dataTable { get; set; }
 
        public override object Clone()
        {
            var clone = (FkComboCell)base.Clone();
            return clone;
        }

        public override void InitializeEditingControl(int rowIndex, object
            initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
        {
            // Set the value of the editing control to the current cell value.
            base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);

            FkComboBoxEditingControl ctl = DataGridView.EditingControl as FkComboBoxEditingControl;
            
            if (ctl != null) { 
                // Fill the combo.
                ctl.DataSource = null;   
                ctl.Items.Clear(); // ? if needed
                ctl.ValueMember = "ValueMember";
                ctl.DisplayMember = "DisplayMember";
                ctl.DataSource = dataTable;
                // Other combo settings
                ctl.AutoCompleteMode = AutoCompleteMode.None;  // Confusing otherwise
            }
        }

        public override Type EditType
        {
            get
            {
                return typeof(FkComboBoxEditingControl);
            }
        }

        public override Type ValueType
        {
            get
            {
                return typeof(Int32);
            }
        }

        public override object DefaultNewRowValue
        {
            get
            {
                return 0;
            }
        }
    }

    public class FkComboBoxEditingControl : DataGridViewComboBoxEditingControl
    {
        public override void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle)
        {
            this.BackColor = Color.Aqua;
        }


    }
}