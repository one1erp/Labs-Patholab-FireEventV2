﻿using LSExtensionWindowLib;
using LSSERVICEPROVIDERLib;
using MSXML;
using Oracle.ManagedDataAccess.Client;
using Patholab_Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FireEventV2
{
    
    [ComVisible(true)]
    [ProgId("FireEventV2.FireEvent")]

    public partial class FireEventCls : UserControl, IExtensionWindow
    {
        #region Ctor

        public FireEventCls()
        {
            try
            {
                InitializeComponent();
                BackColor = Color.FromName("Control");
                _dataItems = new List<DataItem>();
            }
            catch (Exception e)
            {
                Logger.WriteExceptionToLog(e);
            }
        }

        public FireEventCls(IExtensionWindowSite2 site, INautilusServiceProvider sp, string parametersCommandsName)
        {
            InitializeComponent();
            BackColor = Color.FromName("Control");
            _dataItems = new List<DataItem>();
            _sp = sp;
            if (_sp != null)
            {
                _processXml = _sp.QueryServiceProvider("ProcessXML") as NautilusProcessXML;
                _ntlsUser = _sp.QueryServiceProvider("User") as NautilusUser;

            }
            else
            {
                _processXml = null;
            }

            _ntlsSite = (IExtensionWindowSite2)site;
            _ntlsSite.SetWindowInternalName("Fire Event");
            _ntlsSite.SetWindowRegistryName("Fire Event");
            _ntlsSite.SetWindowTitle("Fire Event");
            PreDisplay();

            string sql = $"select parameter2 from lims_sys.command  where name like '{parametersCommandsName}'";
            cmd = new OracleCommand(sql, _connection);

            var res =
                cmd.ExecuteScalar();
            if (res != null)
            {
                var parameters = res.ToString();
                SetParameters(parameters);
            }

        }

        #endregion

        #region Private members



        private INautilusUser _ntlsUser;
        private IExtensionWindowSite2 _ntlsSite;

        private INautilusProcessXML _processXml;

        private INautilusServiceProvider _sp;

        private OracleConnection _connection;

        private OracleCommand cmd;



        private string _tableName;

        private string _eventName;

        private string _barcodeField;

        private string _phraseEntryWhereClause;

        private string _titleName;

        private string _phraseHeaderName;

        private string _fieldForUpdate;

        private string _valueForUpdate;

        private string _phraseEntryQuery;

        private string _entityId;

        private List<DataItem> _dataItems;

        private string _displayFields;

        private Dictionary<string, string> _entityIcons;

        private ImageList _smallImagesList;

        private double sessionId;

        private string _connectionString;

        #endregion

        #region Implementing IExtensionWindow

        private INautilusRecordSet rs;
        public void PreDisplay()
        {
            INautilusDBConnection dbConnection;
            if (_sp != null)
            {
                dbConnection = _sp.QueryServiceProvider("DBConnection") as NautilusDBConnection;
                rs = _sp.QueryServiceProvider("RecordSet") as NautilusRecordSet;
            }
            else
            {
                dbConnection = null;
            }



            _connection = GetConnection(dbConnection);

            if (!string.IsNullOrEmpty(_phraseHeaderName) && !string.IsNullOrEmpty(_phraseEntryWhereClause))
            {
                var pe = GetPhraseEntry(_phraseHeaderName, _phraseEntryWhereClause);
                if (pe == null) { MessageBox.Show("חסרה הגדרת פרמטר בפרייז הרלוונטי"); return; }
                _whereClause = pe.Description ;
            }
            if (!string.IsNullOrEmpty(_phraseEntryQuery))
            {
                _phrase2Execute = GetPhraseEntry(_phraseHeaderName, _phraseEntryQuery);
            }


        }



        public void SetParameters(string parameters)
        {
            try
            {
                if (listViewEntities.Columns.Count <= 0) //while screen is loading
                {
                    int index = 0;
                    var splitedParameters = parameters.Split(';');
                    this._tableName = splitedParameters[index++];
                    this._barcodeField = splitedParameters[index++];
                    this._displayFields = splitedParameters[index++];
                    this._phraseEntryWhereClause = splitedParameters[index++];
                    this._eventName = splitedParameters[index++];
                    this._titleName = splitedParameters[index++];
                    this._phraseHeaderName = splitedParameters[index++];
                    this._phraseEntryQuery = splitedParameters[index++];
                    this._fieldForUpdate = splitedParameters[index++];

                    applicationCode = _eventName;

                    InitControls();
                    LoadPictures();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("לא הוגדרו פרמטרים כראוי,לא ניתן להשתמש בתוכנית");
                Logger.WriteExceptionToLog(e);
            }
        }

        public bool CloseQuery()
        {
            try
            {
                if (cmd != null) cmd.Dispose();
                if (_connection != null) _connection.Close();

                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex);
                return true;
            }
        }

        public void Internationalise()
        {
        }

        public void SetSite(object site)
        {
            _ntlsSite = (IExtensionWindowSite2)site;
            _ntlsSite.SetWindowInternalName("Fire Event");
            _ntlsSite.SetWindowRegistryName("Fire Event");
            _ntlsSite.SetWindowTitle("Fire Event");
        }

        public WindowButtonsType GetButtons()
        {
            return LSExtensionWindowLib.WindowButtonsType.windowButtonsNone;
        }

        public bool SaveData()
        {
            return false;
        }

        public void SaveSettings(int hKey)
        {
        }

        public void Setup()
        {
        }

        public void refresh()
        {

        }

        public WindowRefreshType DataChange()
        {
            return LSExtensionWindowLib.WindowRefreshType.windowRefreshNone;
        }

        public WindowRefreshType ViewRefresh()
        {
            return LSExtensionWindowLib.WindowRefreshType.windowRefreshNone;
        }

        public void SetServiceProvider(object serviceProvider)
        {
            _sp = serviceProvider as NautilusServiceProvider;

            if (_sp != null)
            {
                _processXml = _sp.QueryServiceProvider("ProcessXML") as NautilusProcessXML;
                _ntlsUser = _sp.QueryServiceProvider("User") as NautilusUser;

            }
            else
            {
                _processXml = null;
            }


        }

        public void RestoreSettings(int hKey)
        {

        }

        #endregion

        #region Form Events

        public int imageIndex = 0;
        private string _whereClause = "";
        private Phrase_entry _phrase2Execute;
        private string applicationCode;
        private string sdgLogDesc;

        private void texEditEntity_KeyPress(object sender, KeyPressEventArgs e)
        {
            string sql = "";
            try
            {

                if (e.KeyChar == (char)13 && txtEditEntity.Text != "") //Enter
                {

                    //Checks if it's already in list view
                    if (!ListViewContains())
                    {

                        
                        // Build query
                        if (!string.IsNullOrEmpty(txtEditEntity.Text))
                        { sql = $@"SELECT {_barcodeField}, {(_displayFields != null ? $"{_displayFields}," : string.Empty)} {_tableName}.{_tableName}_ID EntityId, Status IconStatus 
                                  FROM lims_sys.{_tableName}, lims_sys.{_tableName}_USER WHERE {_tableName}.{_tableName}_ID = {_tableName}_USER.{_tableName}_ID AND {_barcodeField} = '{txtEditEntity.Text}'";
                        }


                        //Add condition to query
                        if (!string.IsNullOrEmpty(_whereClause))
                        {
                            sql += $" and  {_whereClause}";
                        }

                        cmd = new OracleCommand(sql, _connection);
                        OracleDataReader reader = cmd.ExecuteReader();

                        //Checks if it exists
                        if (!reader.HasRows)
                        {

                            MessageBox.Show(
                                _tableName + "  " + txtEditEntity.Text +
                                "  does not exist or does not meet the conditions!", "Nautilus",
                                MessageBoxButtons.OK, MessageBoxIcon.Hand);
                            txtEditEntity.Focus();
                        }
                        else
                        {

                            ListViewItem li = null;
                            while (reader.Read())
                            {

                                for (int i = 0; i < reader.FieldCount; i++)
                                {

                                    //First time
                                    if (i == 0)
                                    {
                                        li = new ListViewItem(reader[i].ToString(), imageIndex++);
                                    }
                                    else
                                    {
                                        var obj = reader[i];
                                        li.SubItems.Add(obj.ToString());
                                    }
                                }

                                listViewEntities.Items.Add(li);
                                label1.Text = listViewEntities.Items.Count + " objects";

                                txtEnterdEntity.Text = txtEditEntity.Text;
                                txtEditEntity.Text = string.Empty;

                                if (_entityIcons != null)
                                {
                                    var path = _entityIcons[reader["IconStatus"].ToString()];

                                    //Add icon
                                    _smallImagesList.Images.Add(Bitmap.FromFile(path));
                                    listViewEntities.SmallImageList = _smallImagesList;
                                }
                                _entityId = reader["EntityId"].ToString();
                                _dataItems.Add(new DataItem { ID = _entityId, Name = reader["NAME"].ToString() });
                            }
                            reader.Close();
                            listViewEntities.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

                        }
                    }
                    else
                    {
                        MessageBox.Show(_tableName + " " + txtEditEntity.Text + "  already exists");
                        txtEditEntity.Focus();

                    }
                }
            }
            catch (Exception e1)
            {
                MessageBox.Show("Error" + e1.Message + e1.StackTrace);

                Logger.WriteExceptionToLog(e1);
            }

        }

        private void listViewEntities_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete) //Delete
            {
                //Remove all selected rows
                foreach (ListViewItem item in listViewEntities.SelectedItems)
                {
                    listViewEntities.Items.Remove(item);
                }
                label1.Text = listViewEntities.Items.Count + " objects";
            }
        }


        private bool ListViewContains()
        {
            foreach (ListViewItem item in listViewEntities.Items)
            {
                if (item.SubItems[0].Text == txtEditEntity.Text)
                    return true;
            }
            return false;
        }

        private void Ok_button_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (ListViewItem item in listViewEntities.Items)
                {
                    //gets entity name
                    var barcodeField = item.SubItems[0].Text;
                    DataItem dataItem = _dataItems.FirstOrDefault(x => x.Name == barcodeField);

                    //run event                    
                    if (!string.IsNullOrEmpty(_eventName))
                    {
                        if (_eventName != "NO_EVENT")
                        {
                            RunEvent(barcodeField);
                        }

                        Add2Log(dataItem.ID, dataItem.Name);
                    }
                }
                if (!string.IsNullOrEmpty(_phraseEntryQuery) && !string.IsNullOrEmpty(_fieldForUpdate))
                {


                    foreach (ListViewItem item in listViewEntities.Items)
                    {
                        var barcodeField = item.SubItems[0].Text;
                        DataItem dataItem = _dataItems.FirstOrDefault(x => x.Name == barcodeField);
                        if (dataItem != null)
                        {
                            _valueForUpdate = ExecutePhraseQuery(_phrase2Execute, dataItem.Name);
                            UpdateData(dataItem.ID, _valueForUpdate);
                            Add2Log(dataItem.ID, dataItem.Name);
                        }
                    }
                }
                //Empties the list
                foreach (ListViewItem item in listViewEntities.Items)
                {
                    listViewEntities.Items.Remove(item);
                }
                _dataItems.Clear();
            }
            catch (Exception e1)
            {
                MessageBox.Show("Error" + e1.Message);

                Logger.WriteExceptionToLog(e1);
            }
        }



        private void Close_button_Click(object sender, EventArgs e)
        {
            try
            {


                if (listViewEntities.Items.Count > 0)
                {
                    DialogResult dialogResult = MessageBox.Show("האם אתה בטוח שברצונך לצאת ממסך זה ללא אישור? ", "יציאה",
                        MessageBoxButtons.YesNoCancel);
                    if (dialogResult == DialogResult.Yes)
                    {
                        listViewEntities = null;
                        if (_ntlsSite != null) _ntlsSite.CloseWindow();
                    }
                }
                else
                {
                    listViewEntities = null;
                    if (_ntlsSite != null) _ntlsSite.CloseWindow();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Logger.WriteExceptionToLog(ex);
            }
            finally
            {

            }
        }

        private void SingleResultEntry_Resize(object sender, EventArgs e)
        {
            lblTitle.Location = new Point(panel1.Width / 2 - lblTitle.Width / 2, lblTitle.Location.Y);
            panel1.Location = new Point(Width / 2 - panel1.Width / 2, panel1.Location.Y);


        }

        #endregion

        #region Private methods

        public OracleConnection GetConnection(INautilusDBConnection ntlsCon)
        {
            OracleConnection connection = null;

            if (ntlsCon != null)
            {
                try
                {
                    // Get the ADO.NET connection string from the provided interface
                    string _connectionString = ntlsCon.GetADOConnectionString();

                    // Split the connection string by ';' to extract relevant information
                    var splitted = _connectionString.Split(';');

                    // Initialize a new connection string
                    var cs = "";

                    // Build the connection string excluding the first element (assumed to be the provider)
                    for (int i = 1; i < splitted.Length; i++)
                    {
                        cs += splitted[i] + ';';
                    }


                    // Get the username from the provided interface
                    var username = ntlsCon.GetUsername();



                    // If username is empty, construct a new connection string with default user
                    if (string.IsNullOrEmpty(username))
                    {
                        var serverDetails = ntlsCon.GetServerDetails();
                        cs = "User Id=/;Data Source=" + serverDetails + ";";
                    }

                    // Create a new Oracle connection using the prepared connection string
                    connection = new OracleConnection(cs);


                    // Open the connection to the Oracle database
                    connection.Open();



                    // Get the LIMS user password from the provided interface
                    string limsUserPassword = ntlsCon.GetLimsUserPwd();


                    // Determine the SQL command to set the role based on the presence of LIMS user password
                    string roleCommand = string.IsNullOrEmpty(limsUserPassword) ? "set role lims_user" : "set role lims_user identified by " + limsUserPassword;


                    // Execute the role-setting command on the Oracle connection
                    OracleCommand command = new OracleCommand(roleCommand, connection);

                    command.ExecuteNonQuery();

                    // Get the session ID from the provided interface
                    double _session_id = ntlsCon.GetSessionId();

                    // Prepare and execute the SQL command to connect to the specified session
                    string sSql = string.Format("call lims.lims_env.connect_same_session({0})", _session_id);
                    command = new OracleCommand(sSql, connection);
                    command.ExecuteNonQuery();

                }
                catch (Exception e)
                {
                    // Catch and rethrow any exceptions that occur during the Oracle connection establishment
                    Logger.WriteExceptionToLog(e);
                    throw new Exception("An error occurred while establishing Oracle connection: " + e.Message);
                }

            }

            if (connection == null)
            {
                connection = new OracleConnection("data source=NAUT;password=lims_sys;user id=lims_sys");
                connection.Open();
            }
            return connection;
        }


        private void InitControls()
        {
            VersionValidator.ValidateRefVersions(Assembly.GetExecutingAssembly());
            //Set title
            lblTitle.Text = this._titleName;

            // Add barcodeField column
            listViewEntities.Columns.Add(_barcodeField, _barcodeField, -1, HorizontalAlignment.Left, 0);
            //Add other columns
            var columns = _displayFields.Split(',');
            foreach (var item in columns)
            {
                string uniqueItem = item;
                if (item.Contains(" "))
                {
                    int index = item.IndexOf(" ");
                    if (item.Length > index + 1)
                    {
                        uniqueItem = item.Substring(index + 1);
                    }
                }

                listViewEntities.Columns.Add(uniqueItem, uniqueItem, -2, HorizontalAlignment.Left, 0);
            }

            //Initilaizes list images
            _smallImagesList = new ImageList();
            listViewEntities.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);



        }

        private void LoadPictures()
        {

            var path = Utils.GetResourcePath();



            if (path != null)
            {
                path += "\\";

                _entityIcons = new Dictionary<string, string>
                {
                    { "not status", _tableName + ".ico" },
                    { "A", path + _tableName + "a" + ".ico" },
                    { "C", path + _tableName + "c" + ".ico" },
                    { "P", path + _tableName + "p" + ".ico" },
                    { "I", path + _tableName + "i" + ".ico" },
                    { "R", path + _tableName + "r" + ".ico" },
                    { "S", path + _tableName + "s" + ".ico" },
                    { "U", path + _tableName + "u" + ".ico" },
                    { "V", path + _tableName + "v" + ".ico" },
                    { "X", path + _tableName + "x" + ".ico" }
                };
            }
        }

        private void RunEvent(string entityName)
        {


            //Creates fire event xml
            var doc = Create_XML(entityName);

            //creates object for respone
            var res = new DOMDocument();

            var msg = _processXml.ProcessXMLWithResponse(doc, res);
            if (msg != "")
            {
                try
                {
                    MessageBox.Show(msg);

                    string fn = _eventName + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".xml";
                    doc.save(@"\\VM-NAUTILUS\logs\ONE1PC1518\FireEvent\Doc " + fn);
                    res.save(@"\\VM-NAUTILUS\logs\ONE1PC1518\FireEvent\Res " + fn);

                }
                catch (Exception e)
                {
                    Logger.WriteExceptionToLog(e);
                }
            }
        }

        private DOMDocument Create_XML(string entityName)
        {
            DOMDocument objDom;


            objDom = new DOMDocument();

            //Creates lims request element
            var objLimsElem = objDom.createElement("lims-request");
            objDom.appendChild(objLimsElem);

            // Creates login request element
            var objLoginElem = objDom.createElement("login-request");
            objLimsElem.appendChild(objLoginElem);

            // Creates Entity element
            var objEntityElem = objDom.createElement(_tableName);
            objLoginElem.appendChild(objEntityElem);


            // Creates   find-by-name element
            var objFindByNameElem = objDom.createElement("find-by-name");
            objEntityElem.appendChild(objFindByNameElem);
            objFindByNameElem.text = entityName;


            //Creates fire-event element
            var objFireEvent = objDom.createElement("fire-event");
            objEntityElem.appendChild(objFireEvent);
            objFireEvent.text = _eventName;

            return objDom;
        }

        private Phrase_entry GetPhraseEntry(string phraseHeaderName, string phraseEntryName)
        {

            string sql = "select phrase_description,phrase_info from lims_sys.phrase_entry " +
                         "where phrase_id = (select phrase_id from lims_sys.phrase_header where " +
                         "name = '" + phraseHeaderName +
                         "') and phrase_Name ='" + phraseEntryName + "'";
            cmd = new OracleCommand(sql, _connection);
            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var p = new Phrase_entry();
                var pd = reader["PHRASE_DESCRIPTION"];
                if (pd != null)
                {
                    p.Description = pd.ToString(); ;
                }
                var pinf = reader["PHRASE_INFO"];
                if (pinf != null)
                {
                    p.Information = pinf.ToString(); ;
                }

                reader.Close();
                return p;
            }
            return null;
        }


        private string ExecutePhraseQuery(Phrase_entry phraseEntry, string barcodeField)
        {
            string sql = phraseEntry.Description;

            if (!string.IsNullOrEmpty(sql))
            {


                sql = sql.Replace("#BarcodeValue#", "'" + barcodeField + "'");
                cmd = new OracleCommand(sql, _connection);
                var value = cmd.ExecuteScalar();
                if (value != null)
                {
                    return value.ToString();
                }

            }
            return null;

        }


        private void UpdateData(string id, string value)
        {

            string tableName = _tableName;

            if (_fieldForUpdate.StartsWith("U_") || _fieldForUpdate.StartsWith("u_"))
            {
                tableName = _tableName + "_user";

            }
            string sql = "";
            if (!string.IsNullOrEmpty(_phrase2Execute.Information) && _phrase2Execute.Information == "D")
            {

                sql = "UPDATE lims_sys." + tableName + " SET " + _fieldForUpdate + " = TO_DATE('" + value + "','dd/mm/yyyy HH24:MI:SS') where " +
                 _tableName +
                 "_ID = '" + id + "'";
            }
            else
            {
                sql = "UPDATE lims_sys." + tableName + " SET " + _fieldForUpdate + " = '" + value + "' where " +
                 _tableName +
                 "_ID = '" + id + "'";
            }


            cmd = new OracleCommand(sql, _connection);
            var rowsAffected = cmd.ExecuteNonQuery();
        }

        private void Add2Log(string id, string name)
        {
            string sdgId4Log = "";

            sdgLogDesc = applicationCode != "NO_EVENT" ? $"{applicationCode} {name}" : $"{_titleName} {name}";

            applicationCode = applicationCode == "NO_EVENT" ? _titleName : applicationCode;

            sdgId4Log = _tableName != "SDG" ? GetSdgId(_tableName, id) : id;

            int temp;

            if (sdgId4Log != null && int.TryParse(sdgId4Log, out temp))
            {

                try
                {
                    string sql = $"BEGIN lims.Insert_To_Sdg_Log ({sdgId4Log}, '{applicationCode}', {sessionId.ToString()}, '{sdgLogDesc}'); END;";

                    cmd = new OracleCommand(sql, _connection);

                    var res = cmd.ExecuteNonQuery();

                }
                catch (Exception e)
                {
                    Logger.WriteExceptionToLog(e);
                }
            }
        }

        #endregion



        private void txtEditEntity_Enter(object sender, EventArgs e)
        {
            zLang.English();
        }


        public string GetSdgId(string tableName, string entityId)
        {

            try
            {

                String sql = "";
                switch (_tableName)
                {
                    case "SAMPLE":
                        sql = "SELECT SDG_ID FROM  lims_sys.Sample where sample_id='" + entityId + "'";
                        break;
                    case "ALIQUOT":
                        sql = " SELECT Sample.SDG_ID FROM  lims_sys.Sample where lims_sys. sample.sample_id in(SELECT  lims_sys.aliquot.sample_id FROM  lims_sys.aliquot where  lims_sys.aliquot.aliquot_id='" + entityId + "')";
                        break;
                    case "TEST":
                        sql = "SELECT SDG_ID FROM  lims_sys.Sample where lims_sys.Sample.sample_id in(SELECT lims_sys.aliquot.sample_id FROM  lims_sys.aliquot where aliquot_id in (SELECT lims_sys.test.aliquot_id FROM  lims_sys.test where lims_sys.test.test_id ='" + entityId + "))'";
                        break;
                    default:
                        sql = "";
                        break;
                }
                if (!string.IsNullOrEmpty(sql))
                {
                    cmd = new OracleCommand(sql, _connection);

                    var res =
                        cmd.ExecuteScalar();
                    if (res != null)
                    {

                        var id = res.ToString();
                        return id;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.WriteExceptionToLog(e);
                return null;
            }
            return null;

        }

        private void txtEditEntity_SizeChanged(object sender, EventArgs e)
        {

        }

    }
}


