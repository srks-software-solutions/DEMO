﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TATA_Production_Status
{
    public partial class Form1 : Form
    {
        i_facility_tsalEntities db = new i_facility_tsalEntities();
        public Form1()
        {
            InitializeComponent();
            Method_ForDeleting();
            //string CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
            //GetCorrecteddate(CorrectedDate);

            Timer MyTimer = new Timer();
            MyTimer.Interval = (30 * 1000); // 30 Sec
            MyTimer.Tick += new EventHandler(MyTimer_Tick);
            MyTimer.Start();


            try
            {
                Timer MyTimerDelete = new Timer();
                MyTimerDelete.Interval = (5 * 60 * 1000); // 5 Minutes
                MyTimerDelete.Tick += new EventHandler(MyTimer_ForDeletingFromPCBDAQINTable);
                MyTimerDelete.Start();
            }

            catch (Exception e)
            {
                IntoFile(" Main Catch " + e.ToString());
            }
        }

        private void MyTimer_Tick(object sender, EventArgs e)
        {
            MachineList();
        }


        public  void MachineList()
        {

            #region SHIFT and CorrectedDate

            string Shift = null;
            DataTable dtshift = new DataTable();
            String queryshift = "SELECT ShiftName,StartTime,EndTime FROM [i_facility_tsal].[dbo].shift_master WHERE IsDeleted = 0";
            TataSqlConnection mcp = new TataSqlConnection();
            mcp.open();
            using (SqlDataAdapter dashift = new SqlDataAdapter(queryshift, mcp.msqlConnection))
            {
                dashift.Fill(dtshift);
            }
            mcp.close();

            String[] msgtime = System.DateTime.Now.TimeOfDay.ToString().Split(':');
            TimeSpan msgstime = System.DateTime.Now.TimeOfDay;
            //TimeSpan msgstime = new TimeSpan(Convert.ToInt32(msgtime[0]), Convert.ToInt32(msgtime[1]), Convert.ToInt32(msgtime[2]));
            TimeSpan s1t1 = new TimeSpan(0, 0, 0), s1t2 = new TimeSpan(0, 0, 0), s2t1 = new TimeSpan(0, 0, 0), s2t2 = new TimeSpan(0, 0, 0);
            TimeSpan s3t1 = new TimeSpan(0, 0, 0), s3t2 = new TimeSpan(0, 0, 0), s3t3 = new TimeSpan(0, 0, 0), s3t4 = new TimeSpan(23, 59, 59);
            for (int k = 0; k < dtshift.Rows.Count; k++)
            {
                if (dtshift.Rows[k][0].ToString().Contains("1"))
                {
                    String[] s1 = dtshift.Rows[k][1].ToString().Split(':');
                    s1t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
                    String[] s11 = dtshift.Rows[k][2].ToString().Split(':');
                    s1t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));
                }
                else if (dtshift.Rows[k][0].ToString().Contains("2"))
                {
                    String[] s1 = dtshift.Rows[k][1].ToString().Split(':');
                    s2t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
                    String[] s11 = dtshift.Rows[k][2].ToString().Split(':');
                    s2t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));
                }
                else if (dtshift.Rows[k][0].ToString().Contains("3"))
                {
                    String[] s1 = dtshift.Rows[k][1].ToString().Split(':');
                    s3t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
                    String[] s11 = dtshift.Rows[k][2].ToString().Split(':');
                    s3t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));
                }
            }
            String CorrectedDate = System.DateTime.Now.ToString("yyyy-MM-dd");
            if (msgstime >= s1t1 && msgstime < s1t2)
            {
                Shift = "A";
            }
            else if (msgstime >= s2t1 && msgstime < s2t2)
            {
                Shift = "B";
            }
            else if ((msgstime >= s3t1 && msgstime <= s3t4) || (msgstime >= s3t3 && msgstime < s3t2))
            {
                Shift = "C";
                if (msgstime >= s3t3 && msgstime < s3t2)
                {
                    CorrectedDate = System.DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                }
            }


            #endregion

            DataTable dataHolderj = new DataTable();
            try
            {
                TataSqlConnection mc = new TataSqlConnection();
                mc.open();
                //String sql1 = "Select * from ( select DAQINID ,v.MachineID , d.PCBIPAddress , d.ParamPIN , ParamValue , CreatedOn from [i_facility_tsal].[dbo].pcbdaqin_tbl as d , VW_Join_DetailsANDParam as v "
                //                + " where v.PCBIPAddress = d.PCBIPAddress and v.PinNumber = d.ParamPIN  order by CreatedOn desc ) as T "
                //                + " group by T.PCBIPAddress,T.ParamPIN";

               string  sql1 = "Select d.DAQINID ,v.MachineID , d.PCBIPAddress , d.ParamPIN , ParamValue , CreatedOn from [i_facility_tsal].[dbo].pcbdaqin_tbl as d , VW_Join_DetailsANDParam as v "
                               + " where v.PCBIPAddress = d.PCBIPAddress and v.PinNumber = d.ParamPIN  order by CreatedOn desc";

                using (SqlDataAdapter da1 = new SqlDataAdapter(sql1, mc.msqlConnection))
                {
                    da1.Fill(dataHolderj);
                }
               
               

                DataView dt1 = new DataView(dataHolderj);
                dt1.Sort = "PCBIPAddress ASC,ParamPIN ASC";
                DataTable dtnew = dt1.ToTable();
                
                var qryLatestInterview = from rows in dtnew.AsEnumerable()                                        
                                         group rows by new { PositionID = rows["PCBIPAddress"], CandidateID = rows["ParamPIN"] } into grp
                                         select grp.First();
                dtnew = qryLatestInterview.CopyToDataTable();
                dataHolderj = dtnew;
                mc.close();
            }
            catch (Exception e)
            {
                IntoFile(" pcbdaqin_tbl " + e.ToString());
            }

            using (i_facility_tsalEntities db1 = new i_facility_tsalEntities())
            {
                #region For NON-HAAS Machines => IsLevel != 4

                //var machineList = db1.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsPCB == 1 && m.MachineID == 14); //&& m.MachineID == 24
                var machineList = db1.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsPCB == 1); //&& m.MachineID == 24

                foreach (var machine in machineList)
                {
                    int machineId = machine.MachineID;

                    //dont push if last mode is Breakdown
                    int IsBreakdown = 0;

                    // var lastMode = new tbllivemodedb();
                    using (i_facility_tsalEntities db5 = new i_facility_tsalEntities())
                    {
                        //lastMode = db5.tbllivemodedbs.Where(m => m.IsDeleted == 0 && m.MachineID == machineId).OrderByDescending(m => m.InsertedOn).Take(1).SingleOrDefault();
                        var lastMode = db5.tbllivemodedbs.Where(m => m.IsDeleted == 0 && m.MachineID == machineId).OrderByDescending(m => m.ModeID).FirstOrDefault();
                        if (lastMode != null)
                        {
                            string mode = lastMode.Mode;
                            int modeStatus = lastMode.IsCompleted;
                            if (mode == "BREAKDOWN")
                            {
                                IsBreakdown = 1;
                            }
                            if (mode == "MNT")
                            {
                                IsBreakdown = 1;
                            }
                        }
                    }


                    if (IsBreakdown == 0)
                    {
                        string pcbipaddress = "";
                        var ParamList = new List<pcb_parameters>();
                        using (i_facility_tsalEntities db5 = new i_facility_tsalEntities())
                        {
                            //string pcbipaddress = db.pcb_details.Where(m => m.IsDeleted == 0 && m.MachineID == machineId).Select(m => m.PCBIPAddress).FirstOrDefault();
                            pcbipaddress = db5.pcb_details.Where(m => m.IsDeleted == 0 && m.MachineID == machineId).Select(m => m.PCBIPAddress).FirstOrDefault();
                            ParamList = db5.pcb_parameters.Where(m => m.MachineID == machineId && m.IsDeleted == 0 && m.ParameterType == "PON").ToList();
                        }
                        bool pingable = false;
                        Ping pinger = new Ping();
                        try
                        {
                            PingReply reply = pinger.Send(pcbipaddress);
                            pingable = reply.Status == IPStatus.Success;

                            IntoFile("Pinging Status of " + pcbipaddress + ": " + pingable);
                        }
                        catch (PingException e)
                        {
                            // Discard PingExceptions and return false;
                            IntoFile(" PING Exception" + "MachineID: " + machineId + " " + e.ToString());
                        }
                        catch (Exception e)
                        {
                            IntoFile(" During Ping " + "MachineID: " + machineId + " " + e.ToString());
                        }


                        if (pingable)
                        {
                            #region OLD NO-PING NO-BLUE
                            var NoPingData = new handle_no_ping();
                            using (i_facility_tsalEntities db5 = new i_facility_tsalEntities())
                            {
                                NoPingData = db5.handle_no_ping.Where(m => m.MachineID == machineId).FirstOrDefault();
                            }
                            if (NoPingData != null)
                            {
                                int id = NoPingData.NoPingID;
                                handle_no_ping hnp = db.handle_no_ping.Find(id);
                                db.handle_no_ping.Remove(hnp);
                                db.SaveChanges();
                            }
                            #endregion

                            var dr = dataHolderj.Select("MachineID = '" + @machineId + "'").ToList();
                            if (dr != null)
                            {
                                //Set Default values to All Pins
                                int pin17 = 0, pin18 = 0, pin19 = 0, pin20 = 0, pin22 = 0, pin23 = 0, pin24 = 0, pin25 = 0, pin26 = 0;
                                var Factorlist = new List<KeyValuePair<int, int>>();
                                //list.Add(new KeyValuePair<string, int>("Rabbit", 4));
                                #region Try Catch 1
                                try
                                {
                                    foreach (var rowj in dr)
                                    {
                                        //var jack = rowj;
                                        int pinNo = Convert.ToInt32(rowj[3]);
                                        int pinVal = Convert.ToInt32(rowj[4]);
                                        var paramData = (from row in ParamList where row.PinNumber == pinNo select row);
                                        if (paramData != null)
                                        {
                                            Factorlist.Add(new KeyValuePair<int, int>(pinNo, pinVal));
                                        }

                                        //Now Assign Value taken from pcbdaqin_tbl to PIN's
                                        switch (pinNo)
                                        {
                                            case 17:
                                                {
                                                    pin17 = pinVal;
                                                    break;
                                                }
                                            case 18:
                                                {
                                                    pin18 = pinVal;
                                                    break;
                                                }
                                            case 19:
                                                {
                                                    pin19 = pinVal;
                                                    break;
                                                }
                                            case 20:
                                                {
                                                    pin20 = pinVal;
                                                    break;
                                                }
                                            case 22:
                                                {
                                                    pin22 = pinVal;
                                                    break;
                                                }
                                            case 23:
                                                {
                                                    pin23 = pinVal;
                                                    break;
                                                }
                                            case 24:
                                                {
                                                    pin24 = pinVal;
                                                    break;
                                                }
                                            case 25:
                                                {
                                                    pin25 = pinVal;
                                                    break;
                                                }
                                            case 26:
                                                {
                                                    pin26 = pinVal;
                                                    break;
                                                }
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    IntoFile(" Gen KeyValuePair " + "MachineID: " + machineId + " " + e.ToString());
                                    //MessageBox.Show("Gen KeyValuePair " + e);
                                }

                                #endregion //try catch 1

                                #region Try Catch 2
                                try
                                {
                                    //int PONHighValue = Convert.ToInt32((from row in ParamList where row.ColorCode == "blue" select row.HighValue).SingleOrDefault());
                                    //int PONPinNo = Convert.ToInt32((from row in ParamList where row.ColorCode == "blue" select row.PinNumber).SingleOrDefault());
                                    int PONHighValue = Convert.ToInt32((from row in ParamList where row.ColorCode == "blue" select row.HighValue).FirstOrDefault());
                                    int PONPinNo = Convert.ToInt32((from row in ParamList where row.ColorCode == "blue" select row.PinNumber).FirstOrDefault());

                                    List<int> ponvalue = (from kvp in Factorlist where kvp.Key == PONPinNo select kvp.Value).ToList();

                                    if (ponvalue.Count > 0)
                                    {
                                        int PONValue = ponvalue[0];

                                        if (PONHighValue == PONValue)
                                        {
                                            #region //New Logic to Decide the Color( Row ) into  tbllivemodedb Table.
                                            string ColorIntoTblMode = "yellow";
                                            var ColorData = new pcbdps_master();
                                            using (i_facility_tsalEntities db5 = new i_facility_tsalEntities())
                                            {
                                                ColorData = db.pcbdps_master.Where(m => m.IsDeleted == 0 && m.MachineID == machineId && m.Pin17 == pin17 && m.Pin18 == pin18 && m.Pin19 == pin19 && m.Pin20 == pin20 && m.Pin22 == pin22 && m.Pin23 == pin23 && m.Pin24 == pin24 && m.Pin25 == pin25 && m.Pin26 == pin26).FirstOrDefault();
                                            }
                                            if (ColorData != null)
                                            {
                                                ColorIntoTblMode = ColorData.ColorValue;
                                            }
                                            if (ColorIntoTblMode == "yellow")
                                            {
                                                 InsertingDataIntoModeTable("IDLE", machineId, ColorIntoTblMode, CorrectedDate, Shift);
                                            }
                                            else if (ColorIntoTblMode == "green")
                                            {
                                                 InsertingDataIntoModeTable("PowerOn", machineId, ColorIntoTblMode, CorrectedDate, Shift);
                                            }
                                            #endregion
                                        }
                                        else
                                        {
                                            #region Check in tbllivemodedb and push. TODO

                                            var lastMode = new tbllivemodedb();

                                            using (i_facility_tsalEntities db5 = new i_facility_tsalEntities())
                                            {
                                                lastMode = db5.tbllivemodedbs.Where(m => m.IsDeleted == 0 && m.MachineID == machineId).OrderByDescending(m => m.InsertedOn).FirstOrDefault();
                                            }
                                            if (lastMode != null)
                                            {
                                                string mode = lastMode.Mode;
                                                int modeStatus = lastMode.IsCompleted;
                                                if (mode == "BREAKDOWN")
                                                {
                                                     InsertingDataIntoModeTable("BREAKDOWN", machineId, "red", CorrectedDate, Shift);
                                                }
                                                else if (mode == "MNT")
                                                {
                                                     InsertingDataIntoModeTable("MNT", machineId, "red", CorrectedDate, Shift);
                                                }
                                                else
                                                {
                                                     InsertingDataIntoModeTable("PowerOff", machineId, "blue", CorrectedDate, Shift);
                                                }
                                            }
                                            else
                                            {
                                                 InsertingDataIntoModeTable("PowerOff", machineId, "blue", CorrectedDate, Shift);
                                            }
                                            #endregion
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    IntoFile(" After KeyGen " + "MachineID: " + machineId + " " + e.ToString());
                                    //MessageBox.Show("After KeyGen: " + e);
                                }
                                #endregion  //end of try catch 2
                            }
                        }
                        else //if not pingable
                        {
                             InsertingDataIntoModeTable("PowerOff", machineId, "blue", CorrectedDate, Shift);
                        }
                    }
                }
                #endregion
            }
        }


        public  bool InsertingDataIntoModeTable(string mode, int machineId, string color, string CorrectedDate, string Shift)
        {
            bool tic = true;
            //Based on the machineid and correctedDate select the last(Latest) mode in mode table 
            // see if new mode is equal to last(latest) mode , if not insert.

            //Take last mode from today insert it now. or latest of modes from latest of previous days . Only if current mode is different from new mode.

            //var lastMode = db.tbllivemodedbs.Where(m => m.IsDeleted == 0 && m.MachineID == machineId && m.CorrectedDate == CorrectedDate).OrderByDescending(m => m.InsertedOn).Take(1).SingleOrDefault();

            #region cheking the day for completion

            List<tbllivemodedb> lastMode = new List<tbllivemodedb>();
            tbllivemodedb lastmodesinglerow = new tbllivemodedb();

            using (i_facility_tsalEntities db5 = new i_facility_tsalEntities())
            {
                //lastMode = db5.tbllivemodedbs.Where(m => m.IsDeleted == 0 && m.MachineID == machineId && m.CorrectedDate == CorrectedDate).OrderByDescending(m => m.ModeID).FirstOrDefault();
                lastMode = db5.tbllivemodedbs.Where(m => m.IsDeleted == 0 && m.MachineID == machineId && m.CorrectedDate == CorrectedDate && m.IsCompleted == 0).OrderByDescending(m => m.ModeID).ToList();
            }

            if (lastMode != null && lastMode.Count == 1)
            {
                using (i_facility_tsalEntities db5 = new i_facility_tsalEntities())
                {
                    //lastMode = db5.tbllivemodedbs.Where(m => m.IsDeleted == 0 && m.MachineID == machineId && m.CorrectedDate == CorrectedDate).OrderByDescending(m => m.ModeID).FirstOrDefault();
                    lastmodesinglerow = db5.tbllivemodedbs.Where(m => m.IsDeleted == 0 && m.MachineID == machineId && m.CorrectedDate == CorrectedDate && m.IsCompleted == 0).OrderByDescending(m => m.ModeID).FirstOrDefault();
                }

                if (lastmodesinglerow.Mode != mode)
                {
                    try
                    {
                        //update endtime for last mode 
                        DateTime dt = DateTime.Now;
                        int lastmodeID = lastmodesinglerow.ModeID;

                        //get colorcode
                        //string color = null;
                        if (lastmodeID != 0)
                        {
                            tbllivemodedb tmprevious = new tbllivemodedb();

                            //v changes

                            DateTime CompletedModeET = DateTime.Now;
                            using (i_facility_tsalEntities db5 = new i_facility_tsalEntities())
                            {
                                // getting the last completed endtime validete with the present start time
                                CompletedModeET = Convert.ToDateTime(db5.tbllivemodedbs.Where(m => m.IsDeleted == 0 && m.IsCompleted == 1 && m.MachineID == machineId).OrderByDescending(m => m.ModeID).Select(m => m.EndTime).FirstOrDefault());
                                tmprevious = db.tbllivemodedbs.Find(lastmodeID);
                            }
                            DateTime PresentModeST = Convert.ToDateTime(tmprevious.StartTime);

                            if (PresentModeST == CompletedModeET)
                            {
                                string previousmode = tmprevious.Mode;
                                if (mode != previousmode)
                                {
                                    //using (i_facility_tsalEntities db5 = new i_facility_tsalEntities())
                                    //{
                                    //    tbllivemodedb tmlastmode = db5.tbllivemodedbs.Find(lastmodeID);
                                    //    tmlastmode.EndTime = dt;
                                    //    tmlastmode.IsCompleted = 1;
                                    //    db5.Entry(tmlastmode).State = System.Data.Entity.EntityState.Modified;
                                    //    db5.SaveChanges();
                                    //}

                                    UpdateModeRow(dt, lastmodeID, machineId).Wait();

                                    if (mode == "PowerOff")
                                    {
                                        color = "blue";
                                    }
                                    else if (mode == "PowerOn")
                                    {
                                        color = "green";
                                    }
                                    else if (mode == "IDLE")
                                    {
                                        color = "yellow";
                                    }

                                    //tbllivemodedb tm = new tbllivemodedb();
                                    //tm.CorrectedDate = CorrectedDate;
                                    //tm.InsertedBy = 1;
                                    //tm.InsertedOn = dt;
                                    //tm.StartTime = dt;
                                    //tm.ColorCode = color;
                                    //tm.IsDeleted = 0;
                                    //tm.IsCompleted = 0;
                                    //tm.MachineID = machineId;
                                    //tm.Mode = mode;
                                    //using (i_facility_tsalEntities db5 = new i_facility_tsalEntities())
                                    //{
                                    //    db5.tbllivemodedbs.Add(tm);
                                    //    db5.SaveChanges();
                                    //}

                                    AddModeRow(CorrectedDate, dt, color, machineId, mode).Wait();
                                }
                            }
                            else
                            {
                                string previousmode = tmprevious.Mode;
                                if (mode != previousmode)
                                {
                                    //using (i_facility_tsalEntities db5 = new i_facility_tsalEntities())
                                    //{
                                    //    tbllivemodedb tmlastmode = db5.tbllivemodedbs.Find(lastmodeID);
                                    //    tmlastmode.StartTime = CompletedModeET;
                                    //    tmlastmode.EndTime = dt;
                                    //    tmlastmode.IsCompleted = 1;
                                    //    db5.Entry(tmlastmode).State = System.Data.Entity.EntityState.Modified;
                                    //    db5.SaveChanges();
                                    //}

                                    //using (i_facility_tsalEntities db5 = new i_facility_tsalEntities())
                                    //{


                                    //}

                                    UpdateModeRowWithStartTime(dt, lastmodeID, machineId, CompletedModeET).Wait();
                                    if (mode == "PowerOff")
                                    {
                                        color = "blue";
                                    }
                                    else if (mode == "PowerOn")
                                    {
                                        color = "green";
                                    }
                                    else if (mode == "IDLE")
                                    {
                                        color = "yellow";
                                    }

                                    //tbllivemodedb tm = new tbllivemodedb();
                                    //tm.CorrectedDate = CorrectedDate;
                                    //tm.InsertedBy = 1;
                                    //tm.InsertedOn = dt;
                                    //tm.StartTime = dt;
                                    //tm.ColorCode = color;
                                    //tm.IsDeleted = 0;
                                    //tm.IsCompleted = 0;
                                    //tm.MachineID = machineId;
                                    //tm.Mode = mode;
                                    //using (i_facility_tsalEntities db5 = new i_facility_tsalEntities())
                                    //{
                                    //    db5.tbllivemodedbs.Add(tm);
                                    //    db5.SaveChanges();
                                    //}


                                    AddModeRow(CorrectedDate, dt, color, machineId, mode).Wait();
                                }
                            }

                        }
                    }
                    catch (Exception e)
                    {
                        IntoFile("In Mode ::  1st Loop " + "MachineID: " + machineId + " " + e.ToString());
                        //MessageBox.Show("In Mode ::  1st Loop " + e);
                    }
                }
            }
            else if (lastMode.Count > 1)
            {
                string getmodequery2 = "SELECT ModeID,StartTime,MacMode From [i_facility_tsal].[dbo].tbllivemodedb WHERE IsCompleted = 0 and MachineID = " + machineId + " and CorrectedDate<='" + CorrectedDate + "' order by ModeID";
                DataTable dtModeMultiple = new DataTable();

                using (TataSqlConnection mc = new TataSqlConnection())
                {
                    mc.open();
                    SqlDataAdapter daMode1 = new SqlDataAdapter(getmodequery2, mc.msqlConnection);
                    daMode1.Fill(dtModeMultiple);
                    mc.close();
                }
                using (TataSqlConnection mc = new TataSqlConnection())
                {
                    for (int i = 0; i < (dtModeMultiple.Rows.Count - 1); i++)
                    {
                        if (dtModeMultiple.Rows[i][2].ToString() == dtModeMultiple.Rows[i + 1][2].ToString())
                        {
                            mc.open();
                            SqlCommand cmdpoweroff = new SqlCommand("DELETE FROM [i_facility_tsal].[dbo].tbllivemodedb where ModeID = " + Convert.ToInt32(dtModeMultiple.Rows[i][0]), mc.msqlConnection);
                            int ret = cmdpoweroff.ExecuteNonQuery();
                            mc.close();

                        }
                    }
                }
            }
            else
            {
                List<tbllivemodedb> previousMode = new List<tbllivemodedb>();
                tbllivemodedb previousModesinglerow = new tbllivemodedb();
                using (i_facility_tsalEntities db5 = new i_facility_tsalEntities())
                {
                    //previousMode = db.tbllivemodedbs.Where(m => m.IsDeleted == 0 && m.MachineID == machineId).OrderByDescending(m => m.ModeID).FirstOrDefault();
                    previousMode = db.tbllivemodedbs.Where(m => m.IsDeleted == 0 && m.MachineID == machineId && m.IsCompleted == 0).OrderByDescending(m => m.ModeID).ToList();
                }
                if (previousMode != null && previousMode.Count == 1)
                {
                    previousModesinglerow = db.tbllivemodedbs.Where(m => m.IsDeleted == 0 && m.MachineID == machineId && m.IsCompleted == 0).OrderByDescending(m => m.ModeID).FirstOrDefault();
                    // v condition
                    try
                    {
                        DateTime dt = DateTime.Now;
                        int id = previousModesinglerow.ModeID;

                        //get colorcode
                        //string color = null;
                        if (id != 0)
                        {
                            tbllivemodedb tmprevious = new tbllivemodedb();

                            DateTime CompletedModeET = DateTime.Now;
                            using (i_facility_tsalEntities db5 = new i_facility_tsalEntities())
                            {
                                CompletedModeET = Convert.ToDateTime(db5.tbllivemodedbs.Where(m => m.IsDeleted == 0 && m.IsCompleted == 1 && m.MachineID == machineId).OrderByDescending(m => m.ModeID).Select(m => m.EndTime).FirstOrDefault());
                                tmprevious = db.tbllivemodedbs.Find(id);
                            }
                            DateTime PresentModeST = Convert.ToDateTime(tmprevious.StartTime);

                            if (PresentModeST == CompletedModeET)
                            {
                                string previousmode = tmprevious.Mode;
                                //if (mode != previousmode)
                                //{
                                //tmprevious.EndTime = dt;
                                //tmprevious.IsCompleted = 1;
                                //try
                                //{
                                //    using (i_facility_tsalEntities db5 = new i_facility_tsalEntities())
                                //    {
                                //        db5.Entry(tmprevious).State = System.Data.Entity.EntityState.Modified;
                                //        db5.SaveChanges();
                                //    }
                                //}
                                //catch (Exception e)
                                //{
                                //    IntoFile("MachineID: " + machineId + ". InsetingDataIntoModeTable ::  " + e.ToString());
                                //}


                                UpdateModeRow(dt, id, machineId).Wait();


                                //tbllivemodedb tm = new tbllivemodedb();
                                //tm.CorrectedDate = CorrectedDate;
                                //tm.InsertedBy = 1;
                                //tm.InsertedOn = dt;
                                //tm.StartTime = dt;
                                //tm.ColorCode = previousMode.ColorCode;
                                //tm.IsDeleted = 0;
                                //tm.IsCompleted = 0;
                                //tm.MachineID = machineId;
                                //tm.Mode = previousMode.Mode;
                                //try
                                //{
                                //    using (i_facility_tsalEntities db5 = new i_facility_tsalEntities())
                                //    {
                                //        db5.tbllivemodedbs.Add(tm);
                                //        db5.SaveChanges();
                                //    }
                                //}
                                //catch (Exception e)
                                //{
                                //    IntoFile("MachineID: " + machineId + ". InsertingDataIntoModeTable 921 ::  " + e.ToString());
                                //}
                                //}


                                AddModeRow(CorrectedDate, dt, previousModesinglerow.ColorCode, machineId, previousModesinglerow.Mode).Wait();

                            }
                            else
                            {
                                string previousmode = tmprevious.Mode;
                                //if (mode != previousmode)
                                //{
                                //using (i_facility_tsalEntities db5 = new i_facility_tsalEntities())

                                //{
                                //    tbllivemodedb tmlastmode = db5.tbllivemodedbs.Find(id);
                                //    tmlastmode.StartTime = CompletedModeET;
                                //    tmlastmode.EndTime = dt;
                                //    tmlastmode.IsCompleted = 1;
                                //    db5.Entry(tmlastmode).State = System.Data.Entity.EntityState.Modified;
                                //    db5.SaveChanges();
                                //}

                                UpdateModeRowWithStartTime(dt, id, machineId, CompletedModeET).Wait();
                                if (mode == "PowerOff")
                                {
                                    color = "blue";
                                }
                                else if (mode == "PowerOn")
                                {
                                    color = "green";
                                }
                                else if (mode == "IDLE")
                                {
                                    color = "yellow";
                                }

                                //tbllivemodedb tm = new tbllivemodedb();
                                //tm.CorrectedDate = CorrectedDate;
                                //tm.InsertedBy = 1;
                                //tm.InsertedOn = dt;
                                //tm.StartTime = dt;
                                //tm.ColorCode = color;
                                //tm.IsDeleted = 0;
                                //tm.IsCompleted = 0;
                                //tm.MachineID = machineId;
                                //tm.Mode = mode;
                                //using (i_facility_tsalEntities db5 = new i_facility_tsalEntities())
                                //{
                                //    db5.tbllivemodedbs.Add(tm);
                                //    db5.SaveChanges();
                                //}
                                //}

                                AddModeRow(CorrectedDate, dt, color, machineId, mode).Wait();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        IntoFile("MachineID: " + machineId + ". In Mode ::  2nd Loop " + e.ToString());
                        //MessageBox.Show("In Mode ::  2st Loop " + e);
                    }

                }
                else if (previousMode.Count > 1)
                {
                    try
                    {
                        DataTable dtMode1 = new DataTable();
                        String getmodequery1 = "SELECT ModeID,StartTime From [i_facility_tsal].[dbo].tbllivemodedb WHERE IsCompleted = 0 and MachineID = " + machineId + " order by ModeID";
                        using (TataSqlConnection mc = new TataSqlConnection())
                        {
                            mc.open();
                            SqlDataAdapter daMode1 = new SqlDataAdapter(getmodequery1, mc.msqlConnection);
                            daMode1.Fill(dtMode1);
                            mc.close();
                        }
                        using (TataSqlConnection mc = new TataSqlConnection())
                        {
                            for (int i = 0; i < (dtMode1.Rows.Count - 2); i++)
                            {
                                if (dtMode1.Rows[i][1].ToString() == DateTime.Now.ToString("yyyy-MM-dd 07:15:00"))
                                {
                                    mc.open();
                                    SqlCommand cmdpoweroff = new SqlCommand("DELETE FROM [i_facility_tsal].[dbo].tbllivemodedb where ModeID = " + Convert.ToInt32(dtMode1.Rows[i][0]), mc.msqlConnection);
                                    int ret = cmdpoweroff.ExecuteNonQuery();
                                    mc.close();

                                }
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        IntoFile(ex.ToString());
                    }
                }
                else // No rows in tbllivemodedb for this machine.
                {
                    DateTime dt = DateTime.Now;
                    //tbllivemodedb tm = new tbllivemodedb();
                    //tm.CorrectedDate = CorrectedDate;
                    //tm.InsertedBy = 1;
                    //tm.InsertedOn = DateTime.Now;
                    //tm.StartTime = DateTime.Now;
                    //tm.ColorCode = color;
                    //tm.IsDeleted = 0;
                    //tm.IsCompleted = 0;
                    //tm.MachineID = machineId;
                    //tm.Mode = mode;
                    //try
                    //{
                    //    using (i_facility_tsalEntities db5 = new i_facility_tsalEntities())
                    //    {
                    //        db5.tbllivemodedbs.Add(tm);
                    //        db5.SaveChanges();
                    //    }
                    //}
                    //catch (Exception e)
                    //{
                    //    IntoFile("MachineID: " + machineId + ". In Mode ::  Last Loop " + e.ToString());
                    //    //MessageBox.Show("In Mode ::  Last Loop " + e);
                    //}
                    AddModeRow(CorrectedDate, dt, color, machineId, mode).Wait();

                }
            }
            #endregion

            return tic;
        }

        public async Task<int> AddModeRow(string CorrectedDate, DateTime dt, string color, int machineId, string mode)
        {
            int Ret = 1;
            tbllivemodedb tm = new tbllivemodedb();
            tm.CorrectedDate = CorrectedDate;
            tm.InsertedBy = 1;
            tm.InsertedOn = dt;
            tm.StartTime = dt;
            tm.ColorCode = color;
            tm.IsDeleted = 0;
            tm.IsCompleted = 0;
            tm.MachineID = machineId;
            tm.Mode = mode;
            try
            {
                using (i_facility_tsalEntities db5 = new i_facility_tsalEntities())
                {
                    db5.tbllivemodedbs.Add(tm);
                    db5.SaveChanges();
                }
            }
            catch (Exception e)
            {
                IntoFile("AddModeRow Method For MachineID:" + machineId + "Error is" + e.ToString());
            }

            return await Task<int>.FromResult(Ret);
        }

        public async Task<int> UpdateModeRow(DateTime dt, int lastmachineid, int machineId)
        {
            int Ret = 1;
            tbllivemodedb tmprevious = new tbllivemodedb();
            using (i_facility_tsalEntities db = new i_facility_tsalEntities())
            {
                tmprevious = db.tbllivemodedbs.Find(lastmachineid);
            }
            tmprevious.EndTime = dt;
            tmprevious.IsCompleted = 1;
            var duationinsec = dt.Subtract(Convert.ToDateTime(tmprevious.StartTime)).TotalSeconds;
            tmprevious.DurationInSec = Convert.ToInt32( duationinsec);
            try
            {
                using (i_facility_tsalEntities db5 = new i_facility_tsalEntities())
                {
                    db5.Entry(tmprevious).State = System.Data.Entity.EntityState.Modified;
                    db5.SaveChanges();
                }
            }
            catch (Exception e)
            {
                IntoFile("MachineID: " + machineId + ". InsetingDataIntoModeTable ::  " + e.ToString());
            }

            return await Task<int>.FromResult(Ret);
        }


        public async Task<int> UpdateModeRowWithStartTime(DateTime dt, int lastmodeID, int machineId, DateTime CompletedModeET)
        {
            int Ret = 1;
            tbllivemodedb tmlastmode = new tbllivemodedb();
            using (i_facility_tsalEntities db = new i_facility_tsalEntities())
            {
                tmlastmode = db.tbllivemodedbs.Find(lastmodeID);
            }
            tmlastmode.StartTime = CompletedModeET;
            tmlastmode.EndTime = dt;
            tmlastmode.IsCompleted = 1;

            var duationinsec = dt.Subtract(Convert.ToDateTime(tmlastmode.StartTime)).TotalSeconds;
            tmlastmode.DurationInSec = Convert.ToInt32(duationinsec);
            try
            {
                using (i_facility_tsalEntities db = new i_facility_tsalEntities())
                {
                    db.Entry(tmlastmode).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                IntoFile("AddModeRow Method For MachineID:" + machineId + "Error is" + e.ToString());
            }

            return await Task<int>.FromResult(Ret);
        }

        private void MyTimer_ForDeletingFromPCBDAQINTable(object sender, EventArgs e)
        {
            Method_ForDeleting();
        }

        public void Method_ForDeleting()
        {
            //NEW Delete Logic 2016-11-03

            DateTime DeleteDataBeforeThisDateTime = DateTime.Now.AddMinutes(-3);
            TataSqlConnection mc = null;
            try
            {
                mc = new TataSqlConnection();
                mc.open();
                //SqlCommand cmdDeleteRows = new SqlCommand("delete from [i_facility_tsal].[dbo].pcbdaqin_tbl where DAQINID NOT IN ( " +
                //                                                " Select DAQINID from ( " +
                //                                                " Select DAQINID from ( " +
                //                                                " select DAQINID ,v.MachineID, d.PCBIPAddress , d.ParamPIN , ParamValue , CreatedOn " +
                //                                                " from pcbdaqin_tbl as d , VW_Join_DetailsANDParam as v " +
                //                                                " where v.PCBIPAddress = d.PCBIPAddress and v.PinNumber = d.ParamPIN  order by CreatedOn desc ) as T " +
                //                                                " group by T.PCBIPAddress,T.ParamPIN ) as k) and CreatedOn < '" + DeleteDataBeforeThisDateTime.ToString("yyyy-MM-dd HH:mm:00") + "' ;", mc.msqlConnection);

                //SqlCommand cmdDeleteRows = new SqlCommand("delete from [i_facility_tsal].[dbo].pcbdaqin_tbl where DAQINID NOT IN(" +
                //                  "select DAQINID from pcbdaqin_tbl as d , VW_Join_DetailsANDParam as v where v.PCBIPAddress = d.PCBIPAddress and v.PinNumber = d.ParamPIN)", mc.msqlConnection);
                //cmdDeleteRows.ExecuteNonQuery();
                mc.close();
                DataTable dataHolderj=new DataTable();
                string sql1 = "Select d.DAQINID ,v.MachineID , d.PCBIPAddress , d.ParamPIN , ParamValue , CreatedOn from [i_facility_tsal].[dbo].pcbdaqin_tbl as d , VW_Join_DetailsANDParam as v "
                              + " where v.PCBIPAddress = d.PCBIPAddress and v.PinNumber = d.ParamPIN  order by CreatedOn desc";

                using (SqlDataAdapter da1 = new SqlDataAdapter(sql1, mc.msqlConnection))
                {
                    da1.Fill(dataHolderj);
                }



                DataView dt1 = new DataView(dataHolderj);
                dt1.Sort = "PCBIPAddress ASC,ParamPIN ASC";
                DataTable dtnew = dt1.ToTable();

                var qryLatestInterview = from rows in dtnew.AsEnumerable()
                                         group rows by new { PositionID = rows["PCBIPAddress"], CandidateID = rows["ParamPIN"] } into grp
                                         select grp.First();
                dtnew = qryLatestInterview.CopyToDataTable();
                int count = dtnew.Rows.Count;
                //var ids = (from r in dtnew.AsEnumerable()
                //           where r.Field<DateTime>("CreatedOn") < DeleteDataBeforeThisDateTime
                //          select r.Field<int>("DAQINID")).ToList<int>();


            }
            catch (Exception ex)
            {
                IntoFile(" Delete PrevData Error Inside: " + ex.ToString());
            }
            finally
            {
                mc.close();
            }

        }

        public void IntoFile(string Msg)
        {
            string path1 = AppDomain.CurrentDomain.BaseDirectory;
            string appPath = Application.StartupPath + @"\TataProductionStatusLogFile.txt";
            using (StreamWriter writer = new StreamWriter(appPath, true)) //true => Append Text
            {
                writer.WriteLine(System.DateTime.Now + ":  " + Msg);
            }
        }

        public string GetCorrecteddate(string CorrectedDate)
        {
            string result = "Nochnage";
            DateTime DayTimingDet = DateTime.Now;
            if (CorrectedDate == Convert.ToString(DayTimingDet.Date))
            {
                if (DayTimingDet.Hour < 6)
                {

                }
            }
           
            //try
            //{
            //    using (i_facility_tsalEntities db = new i_facility_tsalEntities())
            //    {
            //        DAyTimingDet = Convert.ToDateTime(CorrectedDate + " " + Convert.ToString(db.tbldaytimings.Where(m => m.IsDeleted == 0).Select(m => m.EndTime).FirstOrDefault()));
            //    }
            //}
            //catch (Exception e)
            //{

            //}
            //if (DAyTimingDet != null)
            //{
            //    DateTime TodayDate = DateTime.Now;
            //    if (TodayDate < DAyTimingDet)
            //    {
            //        result = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            //    }
            //    else
            //    {
            //        result = DateTime.Now.ToString("yyyy-MM-dd");
            //    }
            //}
            return result;
        }

        private void Form1_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
        {
            // if not restarting the uncomment the line of code which is now commented
            //System.Diagnostics.Process.Start(Application.StartupPath);
            Application.Restart();
        }
    }
}
