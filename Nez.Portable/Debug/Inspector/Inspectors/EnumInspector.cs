using Nez.UI;
using System;
using System.Collections.Generic;


#if DEBUG
namespace Nez
{
    public class EnumInspector : Inspector
    {
        private SelectBox<string> _selectBox;


        public override void Initialize(Table table, Skin skin, float leftCellWidth)
        {
            var label = CreateNameLabel(table, skin, leftCellWidth);

            // gotta get ugly here
            _selectBox = new SelectBox<string>(skin);

            var enumValues = Enum.GetValues(_valueType);
            var enumStringValues = new List<string>();
            foreach (object e in enumValues)
                enumStringValues.Add(e.ToString());
            _selectBox.SetItems(enumStringValues);

            _selectBox.OnChanged += selectedItem => { SetValue(Enum.Parse(_valueType, selectedItem)); };

            table.Add(label);
            table.Add(_selectBox).SetFillX();
        }


        public override void Update() => _selectBox.SetSelected(GetValue<object>().ToString());
    }
}
#endif