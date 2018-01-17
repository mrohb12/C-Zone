using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using RateChange2018.BusinessLayer;
using RateChange2018.MessageString;

namespace RateChange2018
{
    public partial class RateChange : Form
    {
        private DateTime _fromDate;
        private DateTime _toDate;
        private HelperFunction hf;
        private GenericClass genericDbHandler;
        private int _memberCode;
        public static DataTable dtrate;
        private decimal _memberRate;
        public decimal _newRate;
        public decimal _newAmt;
        private string _fat = "";
        private string _shift;
        private string _date;
        public RateChange()
        {
            InitializeComponent();
            hf = new HelperFunction();
            genericDbHandler = new GenericClass();
            update.Location = new Point(this.Width/3 +30 , this.Height + 78);
           
            MilkTransaction objMilk = new MilkTransaction();
            dtrate = objMilk.GetMilkRate(objMilk);
        }
        private DataTable _data;
        private void RateChange_Load(object sender, EventArgs e)
        {
            
            dgvShabhashadData.Height = this.Height -25;
           
           
        }


        #region Calculation

        private void GetTotal(string txtFAT, string txtAnimal, string txtWeight)
        {
            decimal rate = 0;
            decimal perLitreRate = 0;
            decimal SNF = 0;
            decimal commissionCow = 0;
            decimal commissionBuffalo = 0;
            decimal currentWeightInLitre = 0;
            decimal NewRate = 0;
            decimal NewAmt = 0;
            string animalType = "";
            try
            {
                if (!string.IsNullOrEmpty(txtFAT))
                {
                    MilkTransaction objMilkTransaction = new MilkTransaction();

                    objMilkTransaction.Fat = Convert.ToDecimal(txtFAT);
                    _fat = objMilkTransaction.Fat.ToString();


                    if (txtAnimal == "ગાય" || txtAnimal == "Cow")
                    {
                        animalType = "c";
                        //Settings SettingObj = new Settings();
                        //DataTable dtGroup = SettingObj.GetStaticCowFAT();
                        //SettingObj.StaticCowFat = Convert.ToDecimal(dtGroup.Rows[0]["StaticCowFat"]);
                        //decimal abc = Convert.ToDecimal(hf.GujToEng(txtFAT));
                        //if (SettingObj.StaticCowFat <= (abc) && SettingObj.StaticCowFat > 0)
                        //{
                        //    _fatGuj = Convert.ToDecimal(SettingObj.StaticCowFat).ToString();
                        //    _fatt = _fatGuj;
                        //}
                    }

                    if (txtAnimal == "ભેંશ" || txtAnimal == "Buffalo")
                    {
                        animalType = "b";
                    }

                    objMilkTransaction.Animal = animalType;

                    int id = BetweenFAT(Convert.ToDecimal(_fat), animalType);

                    if (dtrate != null && dtrate.Rows.Count > 0 && id >= 0)
                    {
                        rate = Convert.ToDecimal(dtrate.Rows[id]["Rate11"].ToString());
                        _memberRate = rate;

                        SNF = Convert.ToDecimal(8.5);
                        if (txtAnimal == "ગાય" || txtAnimal == "Cow")
                        {
                            commissionCow = Convert.ToDecimal(dtrate.Rows[id]["CommissionCow"].ToString());
                            perLitreRate = (rate * (Convert.ToDecimal(_fat) + (SNF * Convert.ToDecimal(0.66))) / 100 -
                                            commissionCow);
                            perLitreRate = Math.Round(perLitreRate, 2);
                        }

                        if (txtAnimal == "ભેંશ" || txtAnimal == "Buffalo")
                        {
                            decimal BonusStartingFatBuff =
                                Convert.ToDecimal(dtrate.Rows[id]["BonusStartingFatBuff"].ToString());
                            decimal BonusBuffalo = Convert.ToDecimal(dtrate.Rows[id]["BonusBuff"].ToString());
                            decimal fat1 = (Convert.ToDecimal(_fat));
                            if (fat1 <= BonusStartingFatBuff)
                            {
                                commissionBuffalo = Convert.ToDecimal(dtrate.Rows[id]["CommissionBuff"].ToString());
                                perLitreRate = (((Convert.ToDecimal(fat1) * rate) / 100) - commissionBuffalo);
                            }
                            else
                            {
                                commissionBuffalo = Convert.ToDecimal(dtrate.Rows[id]["CommissionBuff"].ToString());
                                perLitreRate = (((BonusStartingFatBuff * rate) / 100) - commissionBuffalo);
                                decimal BonusAmount = (((fat1 - BonusStartingFatBuff) * 10) * BonusBuffalo);
                                perLitreRate = perLitreRate + BonusAmount;
                            }
                        }
                        
                        _newRate = Convert.ToDecimal(perLitreRate);
                  
                        _memberRate = rate;
                    }
                }

                if (!string.IsNullOrEmpty(txtWeight) && rate > 0)
                {
                    NewRate = Convert.ToDecimal(perLitreRate);

                    currentWeightInLitre = Convert.ToDecimal(txtWeight);
                    NewAmt = (currentWeightInLitre * NewRate);
                    _newAmt = Convert.ToDecimal(NewAmt);
                }
            }
            catch (Exception e)
            {
                hf.LogException(e);
                _newRate = 0;
                _newAmt = 0;
            }
        }

        public int BetweenFAT(decimal Fat, string Animaltype)
        {
            for (int i = 0; i < dtrate.Rows.Count; i++)
            {
                if (dtrate.Rows[i]["Milk"].ToString() == Animaltype &&
                    Fat > Convert.ToDecimal(dtrate.Compute("MAX(ToFat)", "Milk ='" + Animaltype + "'")))
                {
                    _fat = Convert.ToDecimal(dtrate.Compute("MAX(ToFat)", "Milk ='" + Animaltype + "'")).ToString();
                    return i;
                }

                if (Convert.ToDecimal(dtrate.Rows[i]["FromFat"].ToString()) <= Fat &&
                    Fat <= Convert.ToDecimal(dtrate.Rows[i]["ToFat"].ToString()) &&
                    dtrate.Rows[i]["Milk"].ToString() == Animaltype)
                {
                    _fat = Fat.ToString();
                    return i;
                }
            }

            return -1;
        }

        #endregion

        #region Search Fileds

        public void BindShabhashadMilkCollectData()
        {
            try
            {
                DataTable dt = dgvGetOldData();
                _data = dt;
                if (_data != null)
                {
                    dgvShabhashadData.DataSource = _data;


                    dgvShabhashadData.Columns["SNF"].Visible = false;
                    dgvShabhashadData.Columns["Dairyrate"].Visible = false;
                    dgvShabhashadData.Columns["Amount"].DefaultCellStyle.BackColor = Color.LightSalmon;
                    dgvShabhashadData.Columns["Rate"].DefaultCellStyle.BackColor = Color.LightSalmon;
                    dgvShabhashadData.Columns["MemberRate"].DefaultCellStyle.BackColor = Color.LightSalmon;

                    dgvShabhashadData.Columns["NewRate"].DefaultCellStyle.BackColor = Color.LightBlue;
                    dgvShabhashadData.Columns["NewMemberRate"].DefaultCellStyle.BackColor = Color.LightBlue;
                    dgvShabhashadData.Columns["NewAmount"].DefaultCellStyle.BackColor = Color.LightBlue;

                    dgvShabhashadData.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dgvShabhashadData.Rows.OfType<DataGridViewRow>().Last().Selected = true;
                    dgvShabhashadData.CurrentCell = dgvShabhashadData.Rows.OfType<DataGridViewRow>().Last().Cells
                        .OfType<DataGridViewCell>().First(); // if first wanted
                }
                else
                {
                    MessageBox.Show("Milk Data Not Found");
                }
            }
            catch (Exception exx)
            {
                hf.LogException(exx);
            }
        }


        public DataTable dgvGetOldData()
        {
            MilkTransaction objMilkTransaction = new MilkTransaction();
            _fromDate = DtpFrom.Value;
            _toDate = DtpTo.Value;

              
            objMilkTransaction.FromDate = Convert.ToDateTime(_fromDate.ToString("yyyy/MM/dd"));

            objMilkTransaction.ToDate = Convert.ToDateTime(_toDate.ToString("yyyy/MM/dd"));
            DataTable dt = objMilkTransaction.GetActiveRecord(objMilkTransaction);
            //if (dt != null && dt.Rows.Count > 0)
            //{
            //    foreach (DataRow row in dt.Rows)
            //    {
            //        GetTotal(row["Fat"].ToString(), row["Animal"].ToString(), row["Quantity"].ToString());

            //        row["NewRate"] = _newRate.ToString("0.00");
            //        row["NewAmount"] = _newAmt.ToString("0.00");
            //        row["NewMemberRate"] = _memberRate.ToString("0.00");
            //    }
            //}

            return dt;
        }


        private void Search_Click(object sender, EventArgs e)
        {
            BindShabhashadMilkCollectData();
        }

        #endregion

        #region Update Fileds

        private void update_Click(object sender, EventArgs e)
        {
            try
            {
                int result;
                result = dgvUpdateOldData();
                if (result > 0)
                    MessageBox.Show("Data Updated Successfully");
                else
                    MessageBox.Show("Some Error Occured");
            }
            catch (Exception exception)
            {
                hf.LogException(exception);
            }
        }

        public int dgvUpdateOldData()
        {
            int updateFlag = 0;
            MilkTransaction objMilkTransaction = new MilkTransaction();

            _fromDate = DtpFrom.Value;
            _toDate = DtpTo.Value;
            int FillUpId = 0;

            objMilkTransaction.FromDate = Convert.ToDateTime(_fromDate.ToString("yyyy/MM/dd"));

            objMilkTransaction.ToDate = Convert.ToDateTime(_toDate.ToString("yyyy/MM/dd"));
            DataTable dt = objMilkTransaction.GetActiveRecord(objMilkTransaction);
            if (dt != null && dt.Rows.Count > 0)
            {
                //foreach (DataRow row in dt.Rows)
                //{
                //    //GetTotal(row["Fat"].ToString(), row["Animal"].ToString(), row["Quantity"].ToString());
                //    //_date = row["Date"].ToString();
                //    //row["NewRate"] = _newRate.ToString();
                //    //row["NewAmount"] = _newAmt.ToString();
                //    //row["NewMemberRate"] = _memberRate.ToString();
                //    //objMilkTransaction.Date = DateTime.Parse(_date);
                //    //objMilkTransaction.Shift = row["Shift"].ToString();
                //     objMilkTransaction.FillUpId = int.Parse(row["SrNo."].ToString());
                //    //[Sr No.]
                //    objMilkTransaction.MemberCode = row["MemberCode"].ToString();
                //    //objMilkTransaction.Amount = Convert.ToDecimal(row["NewAmount"].ToString());

                //    updateFlag = objMilkTransaction.UpdateMilkTransaction(objMilkTransaction);
                //    //objMilkTransaction.Rate = Convert.ToDecimal(row["NewRate"].ToString());
                //    //objMilkTransaction.MemberRate = Convert.ToDecimal(row["NewMemberRate"].ToString());

                    
                //}
                genericDbHandler.SqlBulkUpdate(dt);
                return 1;
            }

            return updateFlag;
        }

        #endregion

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void dgvShabhashadData_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}