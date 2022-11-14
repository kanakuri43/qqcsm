using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Xml.Linq;
using System.Xml.XPath;

namespace qqcs.ViewModels
{
    public class QueryFormViewModel : BindableBase
    {
        private XElement _xml;
        private ComboBoxViewModel _selectedQuery = null;
        private ObservableCollection<ComboBoxViewModel> _queries = new ObservableCollection<ComboBoxViewModel>();
        private MySqlConnection _connection;
        private CollectionView _conditionsView;
        private bool _runEnabled = true;
        private bool _saveEnabled = false;
        private DataTable _conditions;
        private DataTable _result;
        private CollectionView _resultCollectionView;

        public QueryFormViewModel()
        {
            LoadSettingFile();
            DatabaseConnection();
            SetQueryName();

            QueriesSelectionChanged = new DelegateCommand<object[]>(QueriesSelectionChangedExecute);
            RunButton = new DelegateCommand(RunButtonExecute).ObservesCanExecute(() => RunEnabled);
            SaveButton = new DelegateCommand(SaveButtonExecute).ObservesCanExecute(() => SaveEnabled);
            SettingButton = new DelegateCommand(SettingButtonExecute);

            SelectedQuery = _queries[0];
            ShowConditionsTable(0);


        }
        public DelegateCommand<object[]> QueriesSelectionChanged { get; }
        public DelegateCommand RunButton { get; }
        public DelegateCommand SaveButton { get; }
        public DelegateCommand SettingButton { get; }


        public ObservableCollection<ComboBoxViewModel> Queries
        {
            get { return _queries; }
            set { SetProperty(ref _queries, value); }

        }
        public ComboBoxViewModel SelectedQuery
        {
            get { return _selectedQuery; }
            set { SetProperty(ref _selectedQuery, value); }
        }

        public CollectionView ConditionsView
        {
            get { return _conditionsView; }
            set { SetProperty(ref _conditionsView, value); }
        }

        public bool RunEnabled
        {
            get { return _runEnabled; }
            set { SetProperty(ref _runEnabled, value); }
        }
        public CollectionView ResultCollectionView
        {
            get { return _resultCollectionView; }
            set { SetProperty(ref _resultCollectionView, value); }
        }

        public bool SaveEnabled
        {
            get { return _saveEnabled; }
            set { SetProperty(ref _saveEnabled, value); }
        }



        private void DatabaseConnection()
        {
            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder();

            var cs = _xml.Element("ConnectString").Value;
            _connection = new MySqlConnection(cs);

            _connection.Open();


        }
        private void LoadSettingFile()
        {

            string currentDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            this._xml = XElement.Load(currentDirectory + "qqcsm.xml");
        }
        private void SetQueryName()
        {
            var querys = (from p in _xml.Elements("Query") select p);
            _queries.Clear();
            foreach (var q in querys)
            {
                _queries.Add(new ComboBoxViewModel(int.Parse(q.Attribute("id").Value), q.Element("Name").Value));

            }
        }
        private void QueriesSelectionChangedExecute(object[] selectedItems)
        {
            try
            {
                var selectedItem = selectedItems[0] as ComboBoxViewModel;
                var id = selectedItem.Value;
                ShowConditionsTable(id);
            }
            catch
            {

            }
        }

        private void ShowConditionsTable(int id)
        {
            var query = (from p in _xml.Elements("Query") where p.Attribute("id").Value == id.ToString() select p).First();
            var dt = new DataTable();
            dt.Columns.Add("Name");
            dt.Columns.Add("Value");
            foreach (var p in query.XPathSelectElements("Param"))
            {
                dt.Rows.Add(p.Element("Name").Value, p.Element("Value").Value);
            }
            _conditions = dt;
            ConditionsView = new BindingListCollectionView(_conditions.AsDataView());
            SaveEnabled = false;

        }
        private void RunButtonExecute()
        {
            var id = SelectedQuery.Value;

            var query = (from q in _xml.Elements("Query") where q.Attribute("id").Value == id.ToString() select q).First();
            string sql = query.Element("SQL").Value;

            // パラメータ セット
            foreach (var p in query.XPathSelectElements("Param"))
            {
                if (sql.Contains("#param" + p.Attribute("id").Value))
                {
                    sql = sql.Replace(("#param" + p.Attribute("id").Value), _conditions.Rows[int.Parse(p.Attribute("id").Value)][1].ToString());
                }
            }

            // Query実行
            using (MySqlCommand command = new MySqlCommand(sql, _connection))
            {
                var addapter = new MySqlDataAdapter(command);
                _result = new DataTable();
                addapter.Fill(_result);
                ResultCollectionView = new BindingListCollectionView(_result.AsDataView());
            }

            // パラメータ保存
            query = (from p in _xml.Elements("Query") where p.Attribute("id").Value == id.ToString() select p).First();
            foreach (var p in query.XPathSelectElements("Param"))
            {
                p.Element("Value").Value = _conditions.Rows[int.Parse(p.Attribute("id").Value)][1].ToString();
            }
            string currentDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            _xml.Save(currentDirectory + "qqcs.xml");

            SaveEnabled = true;
        }

        private void SaveButtonExecute()
        {
            CsvOperator co = new CsvOperator();
            var dialog = new SaveFileDialog();
            dialog.Title = "CSVファイル保存";
            dialog.Filter = "CSVファイル|*.csv";
            dialog.FileName = SelectedQuery.DisplayValue;

            if ((bool)dialog.ShowDialog())
            {
                co.OutputCsv(_result, dialog.FileName, true, ",");
                System.Windows.MessageBox.Show("CSVファイルを保存しました。", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SettingButtonExecute()
        {
            var proc = new System.Diagnostics.Process();

            string currentDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "notepad";
            psi.Arguments = currentDirectory + @"qqcsm.xml";
            Process p = Process.Start(psi);
            p.WaitForExit();

            LoadSettingFile();
            SetQueryName();
            DatabaseConnection();

            SelectedQuery = _queries[0];

            ShowConditionsTable(0);

        }

    }
}
