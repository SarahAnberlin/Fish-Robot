//-----------------------------------------------------------------------
// Copyright (C), 2010, PKU&HNIU
// File Name: ServerControlBoard.cs
// Date: 20101116  Author: LiYoubing  Version: 1
// Description: ����ƽ̨�����Sim2DSvr Dss Serviceʵ���ļ�֮����ServerControlBoardʵ���ļ�
// Histroy:
// Date: 20110512  Author: LiYoubing
// Modification: 
// 1.UnloadStrategy������null���
// Date: 20110518  Author: LiYoubing
// Modification: 
// 1.picMatch_Paint�����Ӷ�̬���ư볡��־��L/R��������ɫ�飩�Ĺ���
// 2.ServerControlBoard_KeyDown�и�Start/Pause/Restart/Replay�Ȱ�ť�����ȼ���Ӧ
// 3.btnRestart_Click�����Ӷ԰볡������־���ļ��ʹ���
// Date: 20110617  Author: LiYoubing
// Modification: 
// 1.btnFishDefaultSetting_Click�лָ����������ͷ���ˮ����ɫ��λ��Ĭ��ֵ
// Date: 20110710  Author: LiYoubing
// Modification: 
// 1.picMatch_Paint����������ʹ������������ƺ͵÷���ʾ��ʽ������Socreֵ������ȷ���Ƿ���ʾ�÷�
// 2.��ӱ������ֹ���
// Date: 20110726  Author: LiYoubing
// Modification: 
// 1.cmbFieldLength_SelectedIndexChanged�н������ʱ�л���Fieldѡ�ʱ��ѷ��������λ�ø�λ��bug
// 2.StateStart��StateRestart�м����Fieldѡ��ϳ��سߴ�������ÿؼ�Enabled״̬��Լ��
// 3.Fieldѡ������ˮ���������
// ����
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Drawing.Drawing2D;
using System.Diagnostics; // for Process
using System.Threading;
using xna = Microsoft.Xna.Framework;

using URWPGSim2D.Common;
using URWPGSim2D.Gadget;
using URWPGSim2D.StrategyLoader;

namespace URWPGSim2D.Sim2DSvr
{
    public partial class ServerControlBoard : Form
    {
        public ServerControlBoard()
        {
            InitializeComponent();

            try
            {
                SysConfig myConfig = SysConfig.Instance();
                _maxCachedCycles = Convert.ToInt32(myConfig.MyXmlReader("MaxCachedCycles"));
            }
            catch
            {
                Console.WriteLine("�������ļ���ȡ��������...");
            }

            _bmpCachedImages = new Bitmap[_maxCachedCycles];
        }

        /// <summary>
        /// ��̬���󣨷��������ͷ���ˮ��ʵʱ��Ϣ��ʾ�Ի���
        /// </summary>
        public DlgFishInfo dlgFishInfo = null;

        /// <summary>
        /// ��̬���󣨷��������ͷ���ˮ���˶��켣���ƶԻ���
        /// </summary>
        public DlgTrajectory dlgTrajectory = null;

        /// <summary>
        /// �ػ泡�ػ�ͼ����PictureBox�ؼ������ݣ�ÿ������������SimulationLoop����
        /// </summary>
        /// <param name="bmp">���ƺþ�̬�Ͷ�̬ͼ�ζ��������������Bitmap��������</param>
        public void DrawMatch(Bitmap bmp)
        {
            #region �洢��ǰ���棬���ڻ���ط�
            if (_curCachingIndex == _maxCachedCycles)
            {
                _curCachingIndex = 0;
            }

            // �˴�Ҫ����Ҫ�����Bitmap�����¡�������ᱻ���ٶ�������
            if (_bmpCachedImages[_curCachingIndex] != null)
            {
                _bmpCachedImages[_curCachingIndex].Dispose();
            }
            _bmpCachedImages[_curCachingIndex] = (Bitmap)bmp.Clone();
            _curCachingIndex++;
            #endregion

            //picMatch.Image = bmp;
            SetMatchBmp(bmp);
        }

        /// <summary>
        /// ���µ�Bitmap���󴫸�PictureBox�ؼ���Image����
        /// ����PictureBox.Image���Ե�ԭֵBitmap��������
        /// </summary>
        /// <param name="bmp">������ΪPictureBox.Image����ֵ��Bitmap��������</param>
        /// <remarks>
        /// �������ص��ڴ�й©���� LiYoubing 20110222
        /// Server���й����У�ռ���ڴ��ɼ�ʮMһֱ������1.3G�������½���һ�ٶ�M��Ȼ�����������������
        /// ������Ϊÿ��Mission.Draw�������µ�Bitmap����CLR�����ڴ����ͺ�
        /// �෬�Ƚ�����ȷ�����½����������д��������
        /// �ڸ�picMatch.Image����ֵǰ����ʽ������ֵ��Ӧ��Bitmap����
        /// </remarks>
        public void SetMatchBmp(Bitmap bmp)
        {
            if (picMatch.Image != null)
            {// �����Ѿ����ڵ�Bitmap����
                picMatch.Image.Dispose();
            }

            // ���µ�Bitmap������
            picMatch.Image = bmp;
        }

        /// <summary>
        /// ���µ�Bitmap���󴫸�PictureBox�ؼ���BackgroundImage����
        /// ����PictureBox.BackgroundImage���Ե�ԭֵBitmap��������
        /// �����ػ���泡��
        /// </summary>
        /// <param name="bmp">������ΪPictureBox.BackgroundImage����ֵ��Bitmap��������</param>
        public void SetFieldBmp(Bitmap bmp)
        {
            // �ػ���泡��
            if (picMatch.BackgroundImage != null)
            {// �����Ѿ����ڵ�Bitmap����
                picMatch.BackgroundImage.Dispose();
            }

            // ���µ�Bitmap������
            picMatch.BackgroundImage = bmp;
        }

        #region ������Referee��忪ʼ/��ͣ/����/�طŰ�ť����
        /// <summary>
        /// ��ʼ�¼���Ӧ����Server����ʵ������START��Ϣ
        /// </summary>
        private void btnStart_Click(object sender, EventArgs e)
        {
            StateStart();

            FromServerUiEvents.Instance().Post(new FromServerUiMsg(FromServerUiMsg.MsgEnum.START,
                new string[] { FromServerUiMsg.MsgEnum.START.ToString() }));
        }

        /// <summary>
        /// ��ͣ�¼���Ӧ����Server����ʵ������PAUSE��Ϣ 
        /// ʹ�佫��ǰʹ����IsRunning����ȡ��
        /// </summary>
        private void btnPause_Click(object sender, EventArgs e)
        {
            // ��ͣ�������ť����
            string strMsg = btnPause.Text.Remove(btnPause.Text.IndexOf(Convert.ToChar("(")));
            MyMission myMission = MyMission.Instance();
            //if (btnPause.Text.Equals("Pause(P)"))
            if (myMission.ParasRef.IsRunning == true && myMission.ParasRef.IsPaused == false)
            {
                btnPause.Text = "Continue(C)";
                //btnReplay.Enabled = true;

                if (MyMission.Instance().IsRomteMode == false)
                {//�����ǰ��localģʽ������ͣģʽ�µ��û������ϵ�ready��ť���ã������û����ز���
                    for (int i = 0;i < _listTeamStrategyControls.Count;i++)
                    {
                        _listTeamStrategyControls[i].BtnBrowse.Enabled = true;
                        _listTeamStrategyControls[i].BtnReady.Enabled = true;
                    }
                    
                }
            }
            //else if (btnPause.Text.Equals("Continue(C)"))
            else if (myMission.ParasRef.IsRunning == true && myMission.ParasRef.IsPaused == true)
            {
                btnPause.Text = "Pause(P)";
                //btnReplay.Enabled = false;

                if (MyMission.Instance().IsRomteMode == false)
                {//�����ǰ��localģʽ�������ģʽ�µ��û������ϵ�ready��ť�����ã��������û����ز���
                    for (int i = 0; i < _listTeamStrategyControls.Count; i++)
                    {
                        _listTeamStrategyControls[i].BtnBrowse.Enabled = false;
                        _listTeamStrategyControls[i].BtnReady.Enabled = false;
                    }
                }
            }

            StatePaused();

            FromServerUiEvents.Instance().Post(new FromServerUiMsg(FromServerUiMsg.MsgEnum.PAUSE,
                new string[] { strMsg }));
        }

        /// <summary>
        /// ģ��������ͣ����ť����Sim2DSvr.ProcessUiUpdating��������
        /// �����ڶԿ��Ա�������򽻻��볡ʱ��Ҫ�ɳ����趨��ͣ״̬ʱ
        /// </summary>
        /// <remarks>added by LiYoubing 20110309</remarks>
        public void ClickPauseBtn()
        {
            btnPause_Click(null, null);
        }

        /// <summary>
        /// �����¼���Ӧ����Server����ʵ������COMPETITION_ITEM_CHANGED��Ϣ 
        /// ʹ��ֹͣ��ǰʹ�����в����³�ʼ����ǰѡ�е�ʹ������
        /// </summary>
        private void btnRestart_Click(object sender, EventArgs e)
        {
            // ˮ�������ʼ��
            InitWaveEffect();

            DialogResult result = MessageBox.Show("Restart Game?", "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                // �ָ���ʱ��Ϊ����ѡ�е�ֵ ����������ʹ���վ�������ʤ��׶Σ�������ָ������ʱ�䣩ʱ��Ҫ
                MyMission.Instance().ParasRef.TotalSeconds = Convert.ToInt32(cmbCompetitionTime.Text) * 60;
                // ���ͱ������͸ı��¼���Ϣ���Ե�ǰѡ�б����������ƺͱ���ʱ���������Ϊ����  modified 20110117
                FromServerUiEvents.Instance().Post(new FromServerUiMsg(FromServerUiMsg.MsgEnum.RESTART,
                    new string[] { FromServerUiMsg.MsgEnum.RESTART.ToString() }));

                // ���ֻ�ܷ���������ܷ���StateRestart�������Ϊֻ���ڵ��Restart��ʱ�Ž������볡��ȫ�����û�ԭ
                // ����StateRestart�����еĻ����ںܶ������������ Liushu 20110513
                MyMission myMission = MyMission.Instance();
                if (myMission.ParasRef.IsExchangedHalfCourt == true
                    && myMission.ParasRef.TeamCount == 2)
                {// �����2֧�����ʹ���ҽ����˰볡�򽻻�����
                    // �����볡ʱ����TeamsRef.Reverse��TeamsRef[0]/TeamsRef[1]ָ����Teams[1]/Teams[0]
                    myMission.TeamsRef.Reverse();

                    // �����˰볡�ı�־��λ
                    myMission.ParasRef.IsExchangedHalfCourt = false;
                }

                StateRestart();

                //added by liushu 20110222
                if (MyMission.Instance().IsRomteMode == false)
                {//�����localģʽ�����Restart֮������Ѽ����˵Ĳ���
                    UnloadStrategy();
                }
                //�������¼���Ӧ��˳��,Ӧ����post��Ϣ,Ȼ��ı䰴ť״̬,Ȼ��ж�ز���.by chenwei
            }
        }

        #region ʹ��ִ�й����оֲ�����طŹ���
        /// <summary>
        /// ����ط�ʹ�ã���ǰ���滭���ڻ��������е�����
        /// </summary>
        private int _curCachingIndex = 0;

        /// <summary>
        /// ����ط�ʹ�ã���ǰ�طŻ����ڻ��������е�����
        /// </summary>
        private int _curReplayingIndex = 0;

        /// <summary>
        /// ����ط�ʹ�ã����������С����󻺴滭������
        /// </summary>
        private int _maxCachedCycles = 50;

        /// <summary>
        /// ����ط�ʹ�ã�����طŻ��������
        /// </summary>
        private Bitmap[] _bmpCachedImages = null;

        private bool _isReplaying = false;

        /// <summary>
        /// �ط��¼���Ӧ����Server����ʵ������REPLAY��Ϣ 
        /// </summary>
        private void btnReplay_Click(object sender, EventArgs e)
        {
            //if (btnReplay.Text == "Replay(Y)")
            if (_isReplaying == false)
            {// ���ڻط� ������ǻطŰ�ť
                _curReplayingIndex = _curCachingIndex;
                //_isReplaying = true;
                btnReplay.Text = "End(E)";
                //btnPause.Enabled = false;
            }
            else
            {// ���ڻط� ������ǽ�����ť
                //_isReplaying = false;
                btnReplay.Text = "Replay(Y)";
                //btnPause.Enabled = true;
            }

            StateReplay();

            FromServerUiEvents.Instance().Post(new FromServerUiMsg(FromServerUiMsg.MsgEnum.REPLAY,
                new string[] { FromServerUiMsg.MsgEnum.REPLAY.ToString() }));
        }

        /// <summary>
        /// ʹ�����й����оֲ�����ط� 
        /// ����ӦREPLAY��Ϣʱ�����TimeoutPort Receiver���շ������ڼ������
        /// </summary>
        public void Replay()
        {
            if (_curReplayingIndex == _maxCachedCycles)
            {
                _curReplayingIndex = 0;
            }
            picMatch.Image = _bmpCachedImages[_curReplayingIndex];
            _curReplayingIndex++;

            if (_isReplaying == true)
            {
                FromServerUiEvents.Instance().Post(new FromServerUiMsg(FromServerUiMsg.MsgEnum.REPLAY,
                    new string[] { FromServerUiMsg.MsgEnum.REPLAY.ToString() }));
            }
        }
        #endregion

        /// <summary>
        /// ����Restart���߽���֮������ڴ����Ѿ����ع��Ĳ��ԡ� added by liushu 20110307
        /// </summary>
        public void UnloadStrategy()
        {          
            for (int i = 0; i < _listTeamStrategyControls.Count; i++)
            {
                if (_listTeamStrategyControls[i]._appDomainForStrategy != null)
                {// ����null���ȷ�����򲻻��쳣 LiYoubing 20110511
                    AppDomain.Unload(_listTeamStrategyControls[i]._appDomainForStrategy);
                }
                _listTeamStrategyControls[i]._appDomainForStrategy = null;
                _listTeamStrategyControls[i].StrategyInterface = null;
            }
            _listTeamStrategyControls.Clear();
        }

        #region �����濪ʼ/��ͣ/����/�ط��ĸ���ť�估��������ؼ����Լ����ϵ����
        /// <summary>
        /// ����ƽ̨��ʼ�󣨷���ͣ״̬����ؿؼ���״̬  weiqingdan20101211
        /// </summary>
        private void StateStart()
        {
            // Refereeѡ�
            cmbCompetitionItem.Enabled = false;
            cmbCompetitionTime.Enabled = false;

            if (MyMission.Instance().IsRomteMode == false)
            {// �����ǰ��localģʽ���������ʼ֮�󣬼��ز��԰�ť������
                for (int i = 0; i < _listTeamStrategyControls.Count; i++)
                {
                    _listTeamStrategyControls[i].BtnBrowse.Enabled = false;
                }
            }

            // ���ø���ťEnabled״̬
            SetBtnEnabledState(false, true, true, false, true, true, true, false, false, false);

            // �����ʼ��ť����������ͷ���ˮ�������ʾ���ÿؼ�����ֻ��״̬
            SetFishAndBallCtrlsReadOnly(true);

            // Fieldѡ����سߴ����ÿؼ����� LiYoubing 20110726
            cmbFieldLength.Enabled = false;
        }

        /// <summary>
        /// ����ƽ̨ ��ͣ/���� ״̬�л�ʱ��ؿؼ���״̬    weiqingdan20101211
        /// </summary>
        private void StatePaused()
        {
            if (MyMission.Instance().ParasRef.IsPaused == true)
            {// ��ǰ����ͣ״̬ ����ļ�����ť �뿪��ͣ״̬
                // ���ø���ťEnabled״̬
                SetBtnEnabledState(false, true, true, false, true, true, true, false, false, false);

                // ���������ť����������ͷ���ˮ�������ʾ���ÿؼ�����ֻ��״̬
                SetFishAndBallCtrlsReadOnly(true);
            }
            else
            {// ��ǰ��δ��ͣ �������ͣ��ť ������ͣ״̬
                // ���ø���ťEnabled״̬
                SetBtnEnabledState(false, true, true, true, true, true, true, false, true, true);

                // �����ͣ��ť����������ͷ���ˮ�������ʾ���ÿؼ����ֻ��״̬
                SetFishAndBallCtrlsReadOnly(false);
            }
        }

        /// <summary>
        /// ����ƽ̨ �ط�/ֹͣ�ط� ״̬�л�ʱ��ؿؼ���״̬    weiqingdan20101211
        /// </summary>
        private void StateReplay()
        {
            // �Ƿ����ڻطű�־��ȡ��
            _isReplaying = !_isReplaying;
            if (_isReplaying == true)
            {// ����ĻطŰ�ť ����ط�״̬
                // ���ø���ťEnabled״̬
                SetBtnEnabledState(false, false, true, true, true, true, true, false, false, false);

                // ���ڻطŷ��������ͷ���ˮ�������ʾ���ÿؼ�����ֻ��״̬
                SetFishAndBallCtrlsReadOnly(true);
            }
            else
            {// ����Ľ�����ť �˳��ط�״̬ �ص���ͣ״̬
                // ���ø���ťEnabled״̬
                SetBtnEnabledState(false, true, true, true, true, true, true, false, true, true);

                // �����ط�һ��������ͣ״̬���������ͷ���ˮ�������ʾ���ÿؼ����ֻ��״̬
                SetFishAndBallCtrlsReadOnly(false);
            }
        }

        /// <summary>
        /// ����ƽ̨����ʱ��ؿؼ���״̬    weiqingdan20101211
        /// </summary>
        private void StateRestart()
        {
            // Refereeѡ�
            cmbCompetitionItem.Enabled = true;
            cmbCompetitionTime.Enabled = true;

            // ���ø���ťEnabled״̬
            SetBtnEnabledState(false, false, false, false, true, true, true, false, true, true);

            // ���������ť����������ͷ���ˮ�������ʾ���ÿؼ����ֻ��״̬
            SetFishAndBallCtrlsReadOnly(false);

            // Fieldѡ����سߴ����ÿؼ����� LiYoubing 20110726
            cmbFieldLength.Enabled = true;

            btnTestVelocity.Text = "Start";
            _isTestMode = false;
        }

        /// <summary>
        /// ����ȫ�����������ͷ���ˮ��Ĳ������ú���ʾ�ؼ��Ƿ�ֻ��
        /// </summary>
        /// <param name="bReadOnly">�Ƿ�ֻ��true��false��</param>
        void SetFishAndBallCtrlsReadOnly(bool bReadOnly)
        {
            for (int i = 0; i < MyMission.Instance().ParasRef.TeamCount; i++)
            {// ����ÿ�����������Ĳ������ú���ʾ�ؼ��Ƿ�ֻ��
                //_listFishSettingControls[i].CmbPlayers.Enabled = true;
                _listFishSettingControls[i].TxtPositionMmX.ReadOnly = bReadOnly;
                _listFishSettingControls[i].TxtPositionMmZ.ReadOnly = bReadOnly;
                _listFishSettingControls[i].TxtDirectionDeg.ReadOnly = bReadOnly;
                _listFishSettingControls[i].BtnConfirm.Enabled = !bReadOnly;
            }

            for (int i = 0; i < MyMission.Instance().EnvRef.Balls.Count; i++)
            {// ����ÿ������ˮ��Ĳ������ú���ʾ�ؼ��Ƿ�ֻ��
                _listBallSettingControls[i].TxtPositionMmX.ReadOnly = bReadOnly;
                _listBallSettingControls[i].TxtPositionMmZ.ReadOnly = bReadOnly;
                _listBallSettingControls[i].TxtRadiusMm.ReadOnly = bReadOnly;
            }
        }

        /// <summary>
        /// �������ؽ��濪ʼ/��ͣ(����)/����/�ط�(����)/¼��/���ƹ켣/��ʾ��Ϣ/
        /// ����/Fish���DefaultSetting/Field���DefaultSetting�ȼ�����ť��Enabled����
        /// </summary>
        private void SetBtnEnabledState(bool bStart, bool bPause, bool bRestart, bool bReplay,
            bool bRecordVideo, bool bDrawTrajectory, bool bDisplayFishInfo,
            bool bTestVelocity, bool bFishDefaultSetting, bool bFieldDefaultSetting)
        {
            btnStart.Enabled = bStart;
            btnPause.Enabled = bPause;
            btnRestart.Enabled = bRestart;
            btnReplay.Enabled = bReplay;
            btnRecordVideo.Enabled = bRecordVideo;
            btnDrawTrajectory.Enabled = bDrawTrajectory;
            btnDisplayFishInfo.Enabled = bDisplayFishInfo;
            btnTestVelocity.Enabled = bTestVelocity;
            btnFishDefaultSetting.Enabled = bFishDefaultSetting;
            btnFieldDefaultSetting.Enabled = bFieldDefaultSetting;
        }
        #endregion
        #endregion

        /// <summary>
        /// �ı���������¼���Ӧ�����³�ʼ��ѡ�е�ʹ��
        /// </summary>
        private void cmbCompetitionItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            // ˮ�������ʼ��
            InitWaveEffect();

            // ����COMPETITION_ITEM_CHANGED��Ϣ���Ե�ǰѡ�б����������ƺͱ���ʱ���������Ϊ����
            // ʹServer����ʵ������InitMission�������³�ʼ����ǰѡ�е�ʹ������
            FromServerUiEvents.Instance().Post(new FromServerUiMsg(FromServerUiMsg.MsgEnum.COMPETITION_ITEM_CHANGED,
                new string[] { ((ComboBox)sender).SelectedItem.ToString(), cmbCompetitionTime.SelectedItem.ToString() }));
        }

        /// <summary>
        /// �ı����ʱ���¼���Ӧ�����ݽ���ѡ����µ�ǰ����ʹ����ʱ����ز���
        /// </summary>
        private void cmbCompetitionTime_SelectedIndexChanged(object sender, EventArgs e)
        {
            int minutes = Int32.Parse(((ComboBox)sender).SelectedItem.ToString());
            if (MyMission.Instance().ParasRef != null)
            {
                // �������ʹ������������
                MyMission.Instance().ParasRef.TotalSeconds = minutes * 60;

                // ���³�ʼ��ʣ��������
                MyMission.Instance().ParasRef.RemainingCycles = minutes * 60 * 1000 
                    / MyMission.Instance().ParasRef.MsPerCycle;

                //picMatch.Image = MyMission.Instance().IMissionRef.Draw();
                SetMatchBmp(MyMission.Instance().IMissionRef.Draw());
            }
        }

        #region ������Referee���Strategy Setting���� Caiqiong
        int _iConnetedClientsCount;
        /// <summary>
        /// ��ǰ�Ѿ������ϵĿͻ�����������ֵΪ0ʱ����������Remoteģʽ��Localģʽ�л�
        /// </summary>
        public int ConnetedClientsCount
        {
            set { _iConnetedClientsCount = value; }
        }

        /// <summary>
        /// Referee���Strategy����̬���ɵ��Զ�����Ͽؼ��б�
        /// </summary>
        List<TeamStrategyComboControls> _listTeamStrategyControls = new List<TeamStrategyComboControls>();
        public List<TeamStrategyComboControls> TeamStrategyControls
        {
            get { return _listTeamStrategyControls; } 
        }

        /// <summary>
        /// �Ƿ��ж����Ѿ�׼���ò����·����Ready��ť�����������Local��Remoteģʽ�л�
        /// </summary>
        bool _isThereTeamReady = false;

        /// <summary>
        /// Ϊ��ǰѡ�е�ʹ�����ͳ�ʼ����Ӧ��Strategy����̬�ؼ�
        /// </summary>
        public void InitTeamStrategyControls()
        {
            pnlStrategy.Controls.Clear();       // ��������Panel����
            _listTeamStrategyControls.Clear();  // �������Ͽؼ��б�  
            for (int i = 0; i < MyMission.Instance().ParasRef.TeamCount; i++)
            {
                InitTeamStrategyControls(i);
            }
        }

        /// <summary>
        /// Ϊ���ΪteamId�Ķ��鴴���Զ�����Ͽؼ�����
        /// </summary>
        /// <param name="teamId">����ı�ţ�0,1...��</param>
        private void InitTeamStrategyControls(int teamId)
        {
            // ������teamId֧�����������Ͽؼ�
            _listTeamStrategyControls.Add(new TeamStrategyComboControls(teamId));

            // �Ѹմ�������Ͽؼ��е�GroupBox������������Panel����
            pnlStrategy.Controls.Add(_listTeamStrategyControls[teamId].GrpContainer);

            // ���øռ���Panel������GroupBox������λ��
            _listTeamStrategyControls[teamId].GrpContainer.Location = new Point(0, teamId * 50);

            // Ϊ��ǰ��Ͽؼ��е�Browse��ť���Click�¼��������ػ涯���Ը��»�����ʾ
            // ��ϸ�ļ��ز��Բ�����TeamStrategyComboControls���BtnBrowse_Click���������
            _listTeamStrategyControls[teamId].BtnBrowse.Click += new EventHandler(BtnBrowse_Click);

            // Ϊ��ǰ��Ͽؼ��е�Ready��ť���Click�¼�����������Ready��ť�����º�Ĳ���
            _listTeamStrategyControls[teamId].BtnReady.Click += new EventHandler(BtnReady_Click);
        }

        ///added by caiqiong 20101204
        /// <summary>
        /// ѡ�񱾵���Ӳ���ģʽʱ��Ľ����л�
        /// </summary>
        private void rdoLocal_Click(object sender, EventArgs e)
        {
            MyMission mission = MyMission.Instance();
            if ((mission.IsRomteMode == true) && (_iConnetedClientsCount == 0))
            {// ��ǰ������Remoteģʽ���޿ͻ��������ϣ������л�ΪLocalģʽ
                mission.IsRomteMode = false;    // �л�ΪLocalģʽ
                _isThereTeamReady = false;
                for (int i = 0; i < _listTeamStrategyControls.Count; i++)
                {
                    _listTeamStrategyControls[i].BtnBrowse.Enabled = true;
                    _listTeamStrategyControls[i].BtnReady.Enabled = false;
                }
            }
            btnTestVelocity.Enabled = true;
            if (mission.IsRomteMode == false) return;
            rdoLocal.Checked = false;
            rdoRemote.Checked = true;
        }

        private void rdoRemote_Click(object sender, EventArgs e)
        {
            MyMission mission = MyMission.Instance();
            if ((mission.IsRomteMode == false) && (_isThereTeamReady == false))
            {// ��ǰ������Localģʽ���޶���Ready�������л�ΪRemoteģʽ
                mission.IsRomteMode = true; // �л�ΪRemoteģʽ
                for (int i = 0; i < _listTeamStrategyControls.Count; i++)
                {
                    _listTeamStrategyControls[i].BtnBrowse.Enabled = false;
                    _listTeamStrategyControls[i].BtnReady.Enabled = true;
                    _listTeamStrategyControls[i].TxtStrategyFileName.Text = "";
                    _listTeamStrategyControls[i].ClearStrategy();
                    //_listTeamStrategyControls[i].ClearStrategy();
                }
            }
            btnTestVelocity.Enabled = false;
            if (mission.IsRomteMode == true) return;
            rdoLocal.Checked = true;
            rdoRemote.Checked = false;
        }
        
        ///add by caiqiong 20101203
        /// <summary>
        /// ����ʹ���������Ready��ť��Ӧ��������������ť��Լ��
        /// </summary>
        public void BtnReady_Click(object sender, EventArgs e)
        {
            // ������"Team1"��Browse��ťName�ַ�����ȡ������1
            //int teamId = Convert.ToInt32(((Button)sender).Name.Substring(4));

            if (MyMission.Instance().IsRomteMode == true) return;
            _isThereTeamReady = true;

            //bool flag = false;
            //for (int i = 0; i < _listTeamStrategyControls.Count; i++)
            //{// ֻҪ����Ready��ť��δ������flagΪtrue
            //    flag = flag || _listTeamStrategyControls[i].BtnReady.Enabled;
            //}
            //if (flag == false)
            //{// ����Ready��ť���ǲ����õ�״̬�����ж��Ƿ����в������鶼�Ѽ��������  added by liushu 20110307
            //    int i;
            //    for (i = 0; i < _listTeamStrategyControls.Count; i++)
            //    {
            //        if (_listTeamStrategyControls[i]._appDomainForStrategy == null)
            //        {
            //            break;
            //        }
            //    }

            //    if (i == _listTeamStrategyControls.Count)       //������еĲ������鶼�Ѿ������������Start��ť����
            //    btnStart.Enabled = true;
            //}

            if (btnPause.Enabled == false)  //localģʽʱ�����ڱ��������е��pause�ǿ������¼��ز������µ��ready�ģ�����Start��Ȼ�����á�
            {//����pause��ť�ǲ����õģ�˵��û�н��б��������ж�Start��ť�Ƿ����
                bool flag = false;
                for (int i = 0; i < _listTeamStrategyControls.Count; i++)
                {// ֻҪ����Ready��ť��δ������flagΪtrue
                    flag = flag || _listTeamStrategyControls[i].BtnReady.Enabled;
                }
                if (flag == false)
                {// ����Ready��ť���ǲ����õ�״̬�����ж��Ƿ����в������鶼�Ѽ��������  added by liushu 20110307
                    int i;
                    for (i = 0; i < _listTeamStrategyControls.Count; i++)
                    {
                        if (_listTeamStrategyControls[i]._appDomainForStrategy == null)
                        {
                            break;
                        }
                    }

                    if (i == _listTeamStrategyControls.Count)       //������еĲ������鶼�Ѿ������������Start��ť����
                        btnStart.Enabled = true;
                }
            }
            else
            {//����Ǳ���״̬��Start��ť������
                btnStart.Enabled = false;
            }

            btnRestart.Enabled = true;
        }

        /// <summary>
        /// ����ʹ������������dll�ļ�Browse��ť��Ӧ��������������ť��Լ�����ػ������ʾ
        /// �������dll�ļ���������ص�IStrategy����ľ��������TeamStrategyComboControls���BtnBrowse_Click���������
        /// </summary>
        public void BtnBrowse_Click(object sender, EventArgs e)
        {
            //// ������"Team1"��Browse��ťName�ַ�����ȡ������1
            //int teamId = Convert.ToInt32(((Button)sender).Name.Substring(4));

            //bool flag = false;
            //for (int i = 0; i < _listTeamStrategyControls.Count; i++)
            //{// ֻҪ����Ready��ť��δ������flagΪtrue
            //    flag = flag || _listTeamStrategyControls[i].BtnReady.Enabled;
            //}
            //btnStart.Enabled = !flag;

            //picMatch.Image = MyMission.Instance().IMissionRef.Draw();
            SetMatchBmp(MyMission.Instance().IMissionRef.Draw());
        }
        #endregion

        #region ������Fish���Fish Setting & Ball Setting���� Liufei
        /// <summary>
        /// Fish���FishSetting����̬���ɵ��Զ�����Ͽؼ��б�
        /// </summary>
        private List<FishSettingComboControls> _listFishSettingControls = new List<FishSettingComboControls>();

        /// <summary>
        /// Fish���BallSetting����̬���ɵ��Զ�����Ͽؼ��б�
        /// </summary>
        private List<BallSettingComboControls> _listBallSettingControls = new List<BallSettingComboControls>();

        /// <summary>
        /// Ϊ��ǰѡ�е�ʹ�����ͳ�ʼ����Ӧ��FishSetting����BallSetting����̬�ؼ�
        /// </summary>
        public void InitFishAndBallSettingControls()
        {
            pnlFishSetting.Controls.Clear();    // ��������Panel����
            _listFishSettingControls.Clear();   // �������Ͽؼ��б�
            for (int i = 0; i < MyMission.Instance().ParasRef.TeamCount; i++)
            {// Ϊÿ֧���鴴����Ͽؼ�
                InitFishSettingControls(i);
            }

            pnlBallSetting.Controls.Clear();    // ��������Panel����
            _listBallSettingControls.Clear();   // �������Ͽؼ��б�
            for (int i = 0; i < MyMission.Instance().EnvRef.Balls.Count; i++)
            {// Ϊÿ������ˮ�򴴽���Ͽؼ�
                InitBallSettingControls(i);
            }
        }

        /// <summary>
        /// Ϊ���ΪteamId�Ķ��鴴���Զ�����Ͽؼ�����
        /// </summary>
        /// <param name="teamId">����ı�ţ�0,1...��</param>
        private void InitFishSettingControls(int teamId)
        {
            // ������teamId֧�����������Ͽؼ�������Ĳ������ڳ�ʼ����Աѡ��ComboBox��Items
            _listFishSettingControls.Add(new FishSettingComboControls(teamId, 
                MyMission.Instance().TeamsRef[teamId].Fishes.Count));

            // �Ѹմ�������Ͽؼ��е�GroupBox������������Panel����
            pnlFishSetting.Controls.Add(_listFishSettingControls[teamId].GrpContainer);

            // ���øռ���Panel������GroupBox������λ��
            _listFishSettingControls[teamId].GrpContainer.Location = new Point(teamId * 100, 5);
            
            // Ϊ��ǰ��Ͽؼ��е�Confirm��ť���Click�¼��������ػ涯���Ը��»�����ʾ
            // �������������õ�������������Ĳ�����FishSettingComboControls���е�Confirm��ťClick�¼���������
            _listFishSettingControls[teamId].BtnConfirm.Click += new EventHandler(BtnConfirm_Click);
        }

        /// <summary>
        /// ���������ɫ��/λ�˵�����Confirm��ť��Ӧ�������ػ�����Ը�����ʾ
        /// ���������õĲ�����������������洢�ľ��������FishSettingComboControls���BtnConfirm_Click���������
        /// </summary>
        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            //picMatch.Image = MyMission.Instance().IMissionRef.Draw();
            SetMatchBmp(MyMission.Instance().IMissionRef.Draw());
        }

        /// <summary>
        /// Ϊ���ΪteamId�Ķ��鴴���Զ�����Ͽؼ�����
        /// </summary>
        /// <param name="teamId">����ı�ţ�0,1...��</param>
        private void InitBallSettingControls(int ballId)
        {
            // ������ballId�ŷ���ˮ���������Ͽؼ�
            _listBallSettingControls.Add(new BallSettingComboControls(ballId, MyMission.Instance().EnvRef.Balls.Count));

            // �Ѹմ�������Ͽؼ��е�GroupBox������������Panel����
            pnlBallSetting.Controls.Add(_listBallSettingControls[ballId].GrpContainer);

            // ���øռ���Panel������GroupBox������λ��
            _listBallSettingControls[ballId].GrpContainer.Location = new Point(0, ballId * 55);

            // Ϊ��ǰ��Ͽؼ��е�TxtPositionMmX/TxtPositionMmZ��������Validated�¼��������ػ涯���Ը��»�����ʾ
            // �������������õ�����ˮ�����Ĳ�����BallSettingComboControls���е���ط��������
            _listBallSettingControls[ballId].TxtPositionMmX.Validated += new EventHandler(TxtPositionMm_Validated);
            _listBallSettingControls[ballId].TxtPositionMmZ.Validated += new EventHandler(TxtPositionMm_Validated);
        }

        /// <summary>
        /// ����ˮ�����������Validated�¼���Ӧ�������ػ涯���Ը��»�����ʾ
        /// �������������õ�����ˮ�����Ĳ�����BallSettingComboControls���е���ط��������
        /// </summary>
        void TxtPositionMm_Validated(object sender, EventArgs e)
        {
            //picMatch.Image = MyMission.Instance().IMissionRef.Draw();
            SetMatchBmp(MyMission.Instance().IMissionRef.Draw());
        }

        /// <summary>
        /// ����Fish���FishSetting����BallSetting����ǰѡ�еķ��������ͷ���ˮ����Ϣ��������
        /// </summary>
        public void ProcessFishAndBallInfoUpdating()
        {
            if (tabServerControlBoard.SelectedIndex == 1)
            {// ��������TabControlѡ�е���Fish���ʱ����Ҫ����
                for (int i = 0; i < _listFishSettingControls.Count; i++)
                {
                    _listFishSettingControls[i].CmbPlayers_SelectedIndexChanged(
                        _listFishSettingControls[i].CmbPlayers, new EventArgs());
                }
                for (int i = 0; i < _listBallSettingControls.Count; i++)
                {
                    _listBallSettingControls[i].SetBallInfoToUi();
                }
            }
        }

        /// <summary>
        /// ����Field���ObstacleSetting����Ϣ��������
        /// </summary>
        private void ProcessFieldInfoUpdating()
        {
            if (tabServerControlBoard.SelectedIndex == 2)
            {// ��������TabControlѡ�е���Field���ʱ����Ҫ����
                cmbObastacleType.SelectedIndex = 1; // �ϰ���������Ϊ����
                lstObstacle.Items.Clear();         // ����б�
                MyMission myMission = MyMission.Instance();
                for (int i = 0; i < myMission.EnvRef.ObstaclesRect.Count; i++)
                {// ����ǰ����ʹ����ȫ�������ϰ���������ʾ���б���
                    lstObstacle.Items.Add(myMission.EnvRef.ObstaclesRect[i].Name);
                }
                if (myMission.EnvRef.ObstaclesRect.Count > 0)
                {// ��ǰ����ʹ�������ϰ���������Ϊ0��ѡ�е�0��
                    lstObstacle.SelectedIndex = 0;
                }
                cmbFieldLength.Text = Field.Instance().FieldLengthXMm.ToString();
                if (cmbObstacleDirection.Text == "")
                {// �ϰ��﷽����Ϊ������ΪĬ��ֵ0
                    cmbObstacleDirection.Text = "0";
                }
            }
        }

        private void tabServerControlBoard_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (tabServerControlBoard.SelectedIndex)
            {
                case 1: // Fishѡ�
                    // ����Fish���FishSetting����ǰѡ�еķ��������ͷ���ˮ����Ϣ��������
                    ProcessFishAndBallInfoUpdating();
                    break;
                case 2: // Fieldѡ�
                    ProcessFieldInfoUpdating();
                    break;
            }
        }

        private void btnFishDefaultSetting_Click(object sender, EventArgs e)
        {// ���³�ʼ����ǰѡ�е�ʹ�����ɻָ����泡���Ϸ��������ͷ���ˮ��Ĭ������
            // LiYoubing 20110617
            IMission iMission = MyMission.Instance().IMissionRef;
            // ������������������Ҫ��λ�����ûָ�Ĭ��ֵ
            iMission.ResetTeamsAndFishes();
            // ���������������ɫ�ͱ����ɫ������ˮ����ɫ�ָ�Ĭ��ֵ
            iMission.ResetColorFishAndId();
            // ����ˮ��ָ�Ĭ��λ��
            iMission.ResetBalls();
            // ����ˮ�����ͱ߿���ɫ�ָ�Ĭ��ֵ
            iMission.ResetColorBall();
            // �ػ���泡���ϵ�ȫ����̬����
            SetMatchBmp(MyMission.Instance().IMissionRef.Draw());
            // ģ���л�ѡ��Ը���Fish�����ʾ
            tabServerControlBoard_SelectedIndexChanged(null, null);
            //// ����COMPETITION_ITEM_CHANGED��Ϣ���Ե�ǰѡ�б����������ƺͱ���ʱ���������Ϊ����
            //// ʹServer����ʵ������InitMission�������³�ʼ����ǰѡ�е�ʹ������
            //FromServerUiEvents.Instance().Post(new FromServerUiMsg(FromServerUiMsg.MsgEnum.COMPETITION_ITEM_CHANGED,
            //    new string[] { ((ComboBox)sender).SelectedItem.ToString(), cmbCompetitionTime.SelectedItem.ToString() }));
        }
        #endregion

        #region ������Fish���Velocity Test����
        private bool _isTestMode = false;
        /// <summary>
        /// ָʾ��ǰ���ڲ���ģʽ���Ѿ�����Velocity Test�µ�Start��ť
        /// </summary>
        public bool IsTestMode
        {
            get { return _isTestMode; }
        }

        public Decision GetTestDecision()
        {
            Decision decision = new Decision();
            decision.VCode = trbVelocityCode.Value;
            decision.TCode = trbSwervingCode.Value;
            return decision;
        }

        private void trbVelocityCode_Scroll(object sender, EventArgs e)
        {
            toolTip.SetToolTip(trbVelocityCode, trbVelocityCode.Value.ToString());
        }

        private void trbSwervingCode_Scroll(object sender, EventArgs e)
        {
            toolTip.SetToolTip(trbSwervingCode, trbSwervingCode.Value.ToString());
        }

        private void btnTestVelocity_Click(object sender, EventArgs e)
        {
            if (btnTestVelocity.Text.Equals("Start"))
            {
                btnTestVelocity.Text = "Stop";
                StateStart();
                btnTestVelocity.Enabled = true;
                _isTestMode = true;

                FromServerUiEvents.Instance().Post(new FromServerUiMsg(FromServerUiMsg.MsgEnum.START,
                    new string[] { FromServerUiMsg.MsgEnum.START.ToString() }));
            }
            else
            {
                btnTestVelocity.Text = "Start";
                btnRestart_Click(btnRestart, new EventArgs());
            }
        }
        #endregion

        #region ������Field���Obstacle Setting���� Added By Caiqiong
        /// <summary>
        /// ����ϰ��� add by caiqiong 2010-11-26
        /// </summary>
        private void btnObstacleAdd_Click(object sender, EventArgs e)
        {
            MyMission myMission = MyMission.Instance();
            xna.Vector3 positionMm = new xna.Vector3(Convert.ToSingle(txtObstaclePosX.Text),
                0, Convert.ToSingle(txtObstaclePosZ.Text));
            int count = 0;
            if (cmbObastacleType.Text.Equals("CIRCLE"))
            {// ���Բ���ϰ���
                count = myMission.EnvRef.ObstaclesRound.Count;
                // ����ǰ����ʹ����Բ���ϰ����б����Ԫ��
                myMission.EnvRef.ObstaclesRound.Add(new RoundedObstacle("CircleObs" + (count + 1), 
                    positionMm, btnObstacleColorBorder.BackColor, btnObstacleColorFilled.BackColor,
                    (int)Convert.ToSingle(txtObstacleLength.Text)));
                // ���Բ���ϰ���ĳߴ��λ�ò����Ϸ��� �����������������������
                RoundedObstacle obj =  myMission.EnvRef.ObstaclesRound[count];
                UrwpgSimHelper.ParametersCheckingObstacle(ref obj);
                // �������б�ؼ����Ԫ��
                lstObstacle.Items.Add(myMission.EnvRef.ObstaclesRound[count].Name);
            }
            else if (cmbObastacleType.Text.Equals("RECTANGLE"))
            {// ��ӷ����ϰ���
                count = myMission.EnvRef.ObstaclesRect.Count;
                // ����ǰ����ʹ���ķ����ϰ����б����Ԫ��
                myMission.EnvRef.ObstaclesRect.Add(new RectangularObstacle("RectObs" + (count + 1),
                    positionMm, btnObstacleColorBorder.BackColor, btnObstacleColorFilled.BackColor,
                    (int)Convert.ToSingle(txtObstacleLength.Text), (int)Convert.ToSingle(txtObstacleWidth.Text), 
                    Convert.ToSingle(cmbObstacleDirection.Text)));
                // ��鷽���ϰ���ĳߴ��λ�ò����Ϸ��� �����������������������
                RectangularObstacle obj = myMission.EnvRef.ObstaclesRect[count];
                UrwpgSimHelper.ParametersCheckingObstacle(ref obj);
                // �������б�ؼ����Ԫ��
                lstObstacle.Items.Add(myMission.EnvRef.ObstaclesRect[count].Name);
            }
            // ����ѡ�ж���ִ��ѡ�������ı��¼���Ӧ����ˢ��ѡ�ж��������ʾ
            lstObstacle.SelectedIndex = count;

            //picMatch.Image = MyMission.Instance().IMissionRef.Draw();
            SetMatchBmp(MyMission.Instance().IMissionRef.Draw());
        }

        /// <summary>
        /// ɾ���ϰ��� add by caiqiong 2010-11-28
        /// </summary>
        private void btnObstacleDelete_Click(object sender, EventArgs e)
        {
            if (lstObstacle.SelectedIndex == -1) return;
            MyMission myMission = MyMission.Instance();
            if (cmbObastacleType.Text.Equals("CIRCLE"))
            {// ɾ��Բ���ϰ���
                // �ӵ�ǰ����ʹ����Բ���ϰ����б�ɾ��Ԫ��
                myMission.EnvRef.ObstaclesRound.RemoveAt(lstObstacle.SelectedIndex);
                // �ӽ����б�ؼ�ɾ��Ԫ��
                lstObstacle.Items.RemoveAt(lstObstacle.SelectedIndex);
            }
            else if (cmbObastacleType.Text.Equals("RECTANGLE"))
            {// ɾ�������ϰ���
                int count = myMission.EnvRef.ObstaclesRect.Count;
                // �ӵ�ǰ����ʹ���ķ����ϰ����б�ɾ��Ԫ��
                myMission.EnvRef.ObstaclesRect.RemoveAt(lstObstacle.SelectedIndex);
                // �ӽ����б�ؼ�ɾ��Ԫ��
                lstObstacle.Items.RemoveAt(lstObstacle.SelectedIndex);
            }
            // ����ѡ�ж���ִ��ѡ�������ı��¼���Ӧ����ˢ��ѡ�ж��������ʾ
            lstObstacle.SelectedIndex = (lstObstacle.Items.Count > 0) ? 0 : -1;
            #region
            //xna.Vector3 positionMm = new xna.Vector3();
            //positionMm.X = Convert.ToInt16(TB_ObstacleOrChannel_X.Text);
            //positionMm.Z = Convert.ToInt16(TB_ObstacleOrChannel_Y.Text);

            //if (RB_Obstacle.Checked == true)
            //{
            //    if (Convert.ToString(cmbObastacleType.SelectedItem) == "CIRCLE")
            //    {
            //        if (lstObstacle.SelectedIndex != -1)
            //        {
            //            int i = Convert.ToInt16(Convert.ToString(lstObstacle.SelectedItem).Trim("CIRCLE".ToCharArray()));
            //            int count = MyMission.Instance().EnvRef.ObstaclesRound.Count;

            //            for (int j = i - 1; j < count - 1; j++)
            //            {
            //                MyMission.Instance().EnvRef.ObstaclesRound[j] = MyMission.Instance().EnvRef.ObstaclesRound[j + 1];
            //            }

            //            lstObstacle.Items.Clear();
            //            for (int j = 1; j < MyMission.Instance().EnvRef.ObstaclesRound.Count; j++)
            //            {
            //                lstObstacle.Items.Add("CIRCLE" + Convert.ToString(j));
            //            }
            //            for (int j = 1; j < MyMission.Instance().EnvRef.ObstaclesRect.Count + 1; j++)
            //            {
            //                lstObstacle.Items.Add("RECTANGLE" + Convert.ToString(j));
            //            }

            //            MyMission.Instance().EnvRef.ObstaclesRound.Remove(MyMission.Instance().EnvRef.ObstaclesRound[count - 1]);
            //        }
            //        else
            //        {
            //            MessageBox.Show("No Obstaclecircle is choosed");
            //        }
            //    }
            //    else if (Convert.ToString(cmbObastacleType.SelectedItem) == "RECTANGLE")
            //    {
            //        if (lstObstacle.SelectedIndex != -1)
            //        {
            //            int i = Convert.ToInt16(Convert.ToString(lstObstacle.SelectedItem).Trim("RECTANGLE".ToCharArray()));
            //            int count = MyMission.Instance().EnvRef.ObstaclesRect.Count;

            //            for (int j = i - 1; j < count - 1; j++)
            //            {
            //                MyMission.Instance().EnvRef.ObstaclesRect[j] = MyMission.Instance().EnvRef.ObstaclesRect[j + 1];
            //            }
            //            //show
            //            lstObstacle.Items.Clear();
            //            for (int j = 1; j < MyMission.Instance().EnvRef.ObstaclesRound.Count + 1; j++)
            //            {
            //                lstObstacle.Items.Add("CIRCLE" + Convert.ToString(j));
            //            }
            //            for (int j = 1; j < MyMission.Instance().EnvRef.ObstaclesRect.Count; j++)
            //            {
            //                lstObstacle.Items.Add("RECTANGLE" + Convert.ToString(j));
            //            }

            //            MyMission.Instance().EnvRef.ObstaclesRect.Remove(MyMission.Instance().EnvRef.ObstaclesRect[count - 1]);
            //        }
            //        else
            //        {
            //            MessageBox.Show("No ObstacleRectangle is choosed");
            //        }
            //    }

            //    //picMatch.Image = MyMission.Instance().IMissionRef.Draw();
            //    SetMatchBmp(MyMission.Instance().IMissionRef.Draw());
            //}
            //else if (RB_Channel.Checked == true)
            //{
            //    int i = Convert.ToInt16(Convert.ToString(List_Channel.SelectedItem).Trim("Channel".ToCharArray()));
            //    int count = MyMission.Instance().EnvRef.Channels.Count;
            //    for (int j = i - 1; j < count - 1; j++)
            //    {
            //        MyMission.Instance().EnvRef.Channels[j] = MyMission.Instance().EnvRef.Channels[j + 1];
            //    }
            //    List_Channel.Items.Clear();
            //    for (int j = 1; j < MyMission.Instance().EnvRef.Channels.Count; j++)
            //    {
            //        List_Channel.Items.Add("Channel" + Convert.ToString(j));
            //    }
            //    MyMission.Instance().EnvRef.Channels.Remove(MyMission.Instance().EnvRef.Channels[count - 1]);
            //}
            #endregion
            //picMatch.Image = MyMission.Instance().IMissionRef.Draw();
            SetMatchBmp(MyMission.Instance().IMissionRef.Draw());
        }

        /// <summary>
        /// �޸��ϰ��� add by caiqiong 2010-11-28
        /// </summary>
        private void btnObstacleModify_Click(object sender, EventArgs e)
        {
            if (lstObstacle.SelectedIndex == -1) return;
            MyMission myMission = MyMission.Instance();
            xna.Vector3 positionMm = new xna.Vector3(Convert.ToSingle(txtObstaclePosX.Text),
                0, Convert.ToSingle(txtObstaclePosZ.Text));
            int index = lstObstacle.SelectedIndex;
            if (cmbObastacleType.Text.Equals("CIRCLE"))
            {// �޸ĵ�ǰ����ʹ����Բ���ϰ����б��ж�Ӧ��ѡ��Ԫ�صĲ���
                myMission.EnvRef.ObstaclesRound[index].PositionMm = positionMm;
                myMission.EnvRef.ObstaclesRound[index].RadiusMm = (int)Convert.ToSingle(txtObstacleLength.Text);
                myMission.EnvRef.ObstaclesRound[index].ColorBorder = btnObstacleColorBorder.BackColor;
                myMission.EnvRef.ObstaclesRound[index].ColorFilled = btnObstacleColorFilled.BackColor;
                // ���Բ���ϰ���ĳߴ��λ�ò����Ϸ��� �����������������������
                RoundedObstacle obj = myMission.EnvRef.ObstaclesRound[index];
                UrwpgSimHelper.ParametersCheckingObstacle(ref obj);
            }
            else if (cmbObastacleType.Text.Equals("RECTANGLE"))
            {// �޸ĵ�ǰ����ʹ���ķ����ϰ����б��ж�Ӧ��ѡ��Ԫ�صĲ���
                myMission.EnvRef.ObstaclesRect[index].PositionMm = positionMm;
                myMission.EnvRef.ObstaclesRect[index].LengthMm = (int)Convert.ToSingle(txtObstacleLength.Text);
                myMission.EnvRef.ObstaclesRect[index].WidthMm = (int)Convert.ToSingle(txtObstacleWidth.Text);
                myMission.EnvRef.ObstaclesRect[index].DirectionRad = xna.MathHelper.ToRadians(Convert.ToSingle(cmbObstacleDirection.Text));
                myMission.EnvRef.ObstaclesRect[index].ColorBorder = btnObstacleColorBorder.BackColor;
                myMission.EnvRef.ObstaclesRect[index].ColorFilled = btnObstacleColorFilled.BackColor;
                // ��鷽���ϰ���ĳߴ��λ�ò����Ϸ��� �����������������������
                RectangularObstacle obj = myMission.EnvRef.ObstaclesRect[index];
                UrwpgSimHelper.ParametersCheckingObstacle(ref obj);
            }
            // ����ѡ�ж���ִ��ѡ�������ı��¼���Ӧ����ˢ��ѡ�ж��������ʾ
            lstObstacle.SelectedIndex = -1;
            lstObstacle.SelectedIndex = index;

            //picMatch.Image = MyMission.Instance().IMissionRef.Draw();
            SetMatchBmp(MyMission.Instance().IMissionRef.Draw());
        }

        /// <summary>
        /// add by caiqiong 2010-11-28
        /// </summary>
        private void cmbObastacleType_SelectedIndexChanged(object sender, EventArgs e)
        {
            // ����ؼ���Ĭ��ֵ
            txtObstaclePosX.Text = "0";
            txtObstaclePosZ.Text = "0";
            txtObstacleLength.Text = "100";
            txtObstacleWidth.Text = "50";
            lstObstacle.Items.Clear();         // ����б�
            MyMission myMission = MyMission.Instance();
            if (cmbObastacleType.Text.Equals("CIRCLE"))
            {
                txtObstacleWidth.Enabled = false;
                cmbObstacleDirection.Enabled = false;
                lblObstacleLength.Text = "Radius:";
                for (int i = 0; i < myMission.EnvRef.ObstaclesRound.Count; i++)
                {// ����ǰ����ʹ����ȫ��Բ���ϰ���������ʾ���б���
                    lstObstacle.Items.Add(myMission.EnvRef.ObstaclesRound[i].Name);
                }
                if (myMission.EnvRef.ObstaclesRound.Count > 0)
                {// ��ǰ����ʹ��Բ���ϰ���������Ϊ0��ѡ�е�0��
                    lstObstacle.SelectedIndex = 0;
                }
            }
            else if (cmbObastacleType.Text.Equals("RECTANGLE"))
            {
                txtObstacleWidth.Enabled = true;
                cmbObstacleDirection.Enabled = true;
                lblObstacleLength.Text = "Length:";
                for (int i = 0; i < myMission.EnvRef.ObstaclesRect.Count; i++)
                {// ����ǰ����ʹ����ȫ�������ϰ���������ʾ���б���
                    lstObstacle.Items.Add(myMission.EnvRef.ObstaclesRect[i].Name);
                }
                if (myMission.EnvRef.ObstaclesRect.Count > 0)
                {// ��ǰ����ʹ�������ϰ���������Ϊ0��ѡ�е�0��
                    lstObstacle.SelectedIndex = 0;
                }
            }
        }

        // LiYoubing 20110629
        /// <summary>
        /// �������ϰ������Ϣ��ʾ������ؼ� add by caiqiong 2010-11-28
        /// </summary>
        /// <param name="obj"></param>
        private void SetRectangularInfoToControls(RectangularObstacle obj)
        {
            txtObstaclePosX.Text = obj.PositionMm.X.ToString();
            txtObstaclePosZ.Text = obj.PositionMm.Z.ToString();
            txtObstacleLength.Text = obj.LengthMm.ToString();
            txtObstacleWidth.Text = obj.WidthMm.ToString();
            cmbObstacleDirection.SelectedItem = xna.MathHelper.ToDegrees(obj.DirectionRad).ToString();
            btnObstacleColorBorder.BackColor = obj.ColorBorder;
            btnObstacleColorFilled.BackColor = obj.ColorFilled;
            btnObstacleDelete.Enabled = obj.IsDeletionAllowed;
            btnObstacleModify.Enabled = obj.IsDeletionAllowed;
        }

        /// <summary>
        /// ��Բ���ϰ������Ϣ��ʾ������ؼ� add by caiqiong 2010-11-28
        /// </summary>
        /// <param name="obj"></param>
        private void SetCircleInfoToControls(RoundedObstacle obj)
        {
            txtObstaclePosX.Text = obj.PositionMm.X.ToString();
            txtObstaclePosZ.Text = obj.PositionMm.Z.ToString();
            txtObstacleLength.Text = obj.RadiusMm.ToString();
            btnObstacleColorBorder.BackColor = obj.ColorBorder;
            btnObstacleColorFilled.BackColor = obj.ColorFilled;
            btnObstacleDelete.Enabled = obj.IsDeletionAllowed;
            btnObstacleModify.Enabled = obj.IsDeletionAllowed;
        }

        /// <summary>
        /// ��Ӧ�ϰ����б�ѡ����ı��¼�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstObstacle_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstObstacle.SelectedIndex == -1) return;
            if (cmbObastacleType.Text.Equals("CIRCLE"))
            {// ��ǰѡ�е���Բ���ϰ���
                SetCircleInfoToControls(MyMission.Instance().EnvRef.ObstaclesRound[lstObstacle.SelectedIndex]);
            }
            else if (cmbObastacleType.Text.Equals("RECTANGLE"))
            {// ��ǰѡ�е��Ƿ����ϰ���
                SetRectangularInfoToControls(MyMission.Instance().EnvRef.ObstaclesRect[lstObstacle.SelectedIndex]);
            }
        }

        /// <summary>
        /// ��Ӧ�ϰ���߿�������ɫ���ð�ť����¼� ���õ�ɫ��������ɫ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnObstacleColor_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            ColorDialog colorDlg = new ColorDialog();
            colorDlg.ShowHelp = true;
            colorDlg.Color = btn.BackColor;

            if (colorDlg.ShowDialog() == DialogResult.OK)
            {// ȷ��ѡ������°�ť����ʾ����ɫ����Ӧ�����ϰ���ı߿���ɫ
                btn.BackColor = colorDlg.Color;
                // û��ѡ���κ��ϰ����޸���ɫ��ť�ı���ɫ�󼴷���
                if (lstObstacle.SelectedIndex == -1) return;
                if (cmbObastacleType.SelectedItem.ToString().Equals("CIRCLE"))
                {// ��ǰ���ڲ�������Բ���ϰ���
                    if (btn.Name.Equals("btnObstacleColorBorder"))
                    {// ��������ϰ���߿���ɫ���ð�ť
                        MyMission.Instance().EnvRef.ObstaclesRound[lstObstacle.SelectedIndex].ColorBorder
                            = btn.BackColor;
                    }
                    else
                    {// ��������ϰ��������ɫ���ð�ť
                        MyMission.Instance().EnvRef.ObstaclesRound[lstObstacle.SelectedIndex].ColorFilled
                           = btn.BackColor;
                    }
                }
                else if (cmbObastacleType.SelectedItem.ToString().Equals("RECTANGLE"))
                {// ��ǰ���ڲ������Ƿ����ϰ���
                    if (btn.Name.Equals("btnObstacleColorBorder"))
                    {// ��������ϰ���߿���ɫ���ð�ť
                        MyMission.Instance().EnvRef.ObstaclesRect[lstObstacle.SelectedIndex].ColorBorder
                            = btn.BackColor;
                    }
                    else
                    {// ��������ϰ��������ɫ���ð�ť
                        MyMission.Instance().EnvRef.ObstaclesRect[lstObstacle.SelectedIndex].ColorFilled
                            = btn.BackColor;
                    }
                }
            }
        }
        #endregion

        #region ������Field���Field Setting����
        private void btnFieldDefaultSetting_Click(object sender, EventArgs e)
        {// �ָ���Ĭ�ϳ��ر���ͬʱ���÷��������/ˮ��/�ϰ���ȵĲ���������ܳ��ֶ��󳬳����ر߽������
            MyMission.Instance().IMissionRef.ResetField();
            MyMission.Instance().IMissionRef.ResetTeamsAndFishes();
            MyMission.Instance().IMissionRef.ResetBalls();
            MyMission.Instance().IMissionRef.ResetObstacles();
            Bitmap bmp = URWPGSim2D.Gadget.WaveEffect.Instance().GetBitmap();
            SetFieldBmp(Field.Instance().Draw(bmp,MyMission.Instance().ParasRef.IsGoalBlockNeeded,MyMission.Instance().ParasRef.IsFieldInnerLinesNeeded));
            SetMatchBmp(MyMission.Instance().IMissionRef.Draw());
            ProcessFieldInfoUpdating();
            // Ĭ�Ͽ���ˮ��Ч�� LiYoubing 20110726 Ĭ�Ϲر� 20110823
            chkWaveEffect.Checked = false;
        }
        #endregion

        #region ���泡�ػ��ƿؼ�PictureBox��MouseMove/MouseLeave/MouseDown/MouseUp���¼� Weiqingdan 20101130
        /// <summary>
        /// picMatch_MouseMove��MouseDown�Ĺ������÷���
        /// �жϵ�ǰ�Ƿ�ѡ�г��ϵķ������������ˮ�����ѡ�������¼����
        /// </summary>
        /// <param name="ptRealField">��ǰ���λ���ڳ��ϵ��Ժ���Ϊ��λ��ʵ������</param>
        /// <param name="isForDragging">trueΪMouseDown���ñ�ʾ��Ҫѡ�й��϶�����ΪMouseMove����</param>
        /// <param name="isBallSelected">��� ��ǰ�Ƿ�ѡ�з���ˮ��</param>
        /// <param name="isFishSelected">��� ��ǰ�Ƿ�ѡ�з��������</param>
        private void DetectRoboFishAndBall(Point ptRealField, bool isForDragging,
            ref bool isBallSelected, ref bool isFishSelected)
        {
            MyMission missionref = MyMission.Instance();
            isBallSelected = false;
            isFishSelected = false;

            // �������ˮ��ѡ��/�϶�
            for (int i = 0; i < missionref.EnvRef.Balls.Count; i++)
            {
                // ��굱ǰλ�õ���i��ˮ�����ĵ�ľ���
                float deltaX = ptRealField.X - missionref.EnvRef.Balls[i].PositionMm.X;
                float deltaZ = ptRealField.Y - missionref.EnvRef.Balls[i].PositionMm.Z;
                double disToBall = Math.Sqrt(deltaX * deltaX + deltaZ * deltaZ);
                if (disToBall < missionref.EnvRef.Balls[i].RadiusMm)
                {
                    if (!_isDragging && isForDragging) _isDragging = true;
                    _selectedBallId = i;//isForDragging ? i : -1;  // ѡ�е�i������ˮ��
                    _selectedTeamId = -1;
                    _selectedFishId = -1;
                    _selectedRoundObstacleId = -1;
                    _selectedRectObstacleId = -1;
                    _selectedChannelId = -1;
                    isBallSelected = true;
                    break;  // �з���ˮ��ѡ�����򲻿���������������ѡ��
                }
            }

            // û�з���ˮ��ѡ�У�����Ҫ����Ƿ��з�������㱻ѡ��/�϶�
            if (isBallSelected == false)
            {
                for (int i = 0; i < missionref.TeamsRef.Count; i++)
                {
                    for (int j = 0; j < missionref.TeamsRef[i].Fishes.Count; j++)
                    {
                        float deltaX = ptRealField.X - missionref.TeamsRef[i].Fishes[j].PositionMm.X;
                        float deltaZ = ptRealField.Y - missionref.TeamsRef[i].Fishes[j].PositionMm.Z;
                        double disToFish = Math.Sqrt(deltaX * deltaX + deltaZ * deltaZ);

                        // �������굱ǰλ���Ƿ�λ�ڷ��������ǰ�˾�����������Բ��
                        if (disToFish < missionref.TeamsRef[i].Fishes[j].FishBodyRadiusMm)
                        {
                            if (!_isDragging && isForDragging) _isDragging = true;
                            _selectedTeamId = i;//isForDragging ? i : -1;  // ��i֧����
                            _selectedFishId = j;//isForDragging ? j : -1;  // ��j����������㱻ѡ��
                            _selectedBallId = -1;
                            _selectedRoundObstacleId = -1;
                            _selectedRectObstacleId = -1;
                            _selectedChannelId = -1;
                            isFishSelected = true;
                            break;  // �з�������㱻ѡ���򲻿���������������ѡ��
                        }
                    }// end for (int j = 0...
                }// end for (int i = 0...
            }// end if (isThereBallSelected...
        }

        /// <summary>
        /// picMatch_MouseMove��MouseDown�Ĺ������÷���
        /// �жϵ�ǰ�Ƿ�ѡ�г��ϵ��ϰ����ͨ�������ѡ�������¼����  added by liushu 20110224
        /// </summary>
        /// <param name="ptRealField">��ǰ���λ���ڳ��ϵ��Ժ���Ϊ��λ��ʵ������</param>
        /// <param name="isForDragging">trueΪMouseDown���ñ�ʾ��Ҫѡ�й��϶�����ΪMouseMove����</param>
        /// <param name="isRoundObstaclesSelected">��� ��ǰ�Ƿ�ѡ�з���Բ���ϰ���</param>
        /// <param name="isRectObstaclesSelected">��� ��ǰ�Ƿ�ѡ�з�������ϰ���</param>
        /// <param name="isFishSelected">��� ��ǰ�Ƿ�ѡ�з���ͨ��</param>
        private void DetectObstaclesAndChannels(Point ptRealField, bool isForDragging,
            ref bool isRoundObstaclesSelected, ref bool isRectObstaclesSelected, ref bool isChannelsSelected)
        {
            MyMission missionref = MyMission.Instance();
            isRoundObstaclesSelected = false;
            isRectObstaclesSelected = false;
            isChannelsSelected = false;

            // �������Բ���ϰ���ѡ��/�϶�
            for (int i = 0; i < missionref.EnvRef.ObstaclesRound.Count; i++)
            {
                // ��굱ǰλ�õ���i��Բ���ϰ������ĵ�ľ���
                float deltaX = ptRealField.X - missionref.EnvRef.ObstaclesRound[i].PositionMm.X;
                float deltaZ = ptRealField.Y - missionref.EnvRef.ObstaclesRound[i].PositionMm.Z;
                double disToRoundObstacle = Math.Sqrt(deltaX * deltaX + deltaZ * deltaZ);

                if (disToRoundObstacle < missionref.EnvRef.ObstaclesRound[i].RadiusMm)
                {
                    if (!_isDragging && isForDragging) _isDragging = true;
                    _selectedRoundObstacleId = i;    //isForDragging ? i : -1;  // ѡ�е�i������Բ���ϰ���
                    _selectedRectObstacleId = -1;
                    _selectedChannelId = -1;
                    _selectedBallId = -1;
                    _selectedTeamId = -1;
                    _selectedFishId = -1;
                    isRoundObstaclesSelected = true;
                    return; // �з���Բ���ϰ��ﱻѡ�����򲻿���������������ѡ��
                }
            }

            // û�з���Բ���ϰ��ﱻѡ�У�����Ƿ��з�������ϰ��ﱻѡ��/�϶�
            if (isRoundObstaclesSelected == false)
            {
                for (int i = 0; i < missionref.EnvRef.ObstaclesRect.Count; i++)
                {
                    if (missionref.EnvRef.ObstaclesRect[i].DirectionRad == 0)
                    {//��������ϰ���ķ����Ϊ0
                        float X1 = missionref.EnvRef.ObstaclesRect[i].PositionMm.X - missionref.EnvRef.ObstaclesRect[i].LengthMm / 2;
                        float X2 = missionref.EnvRef.ObstaclesRect[i].PositionMm.X + missionref.EnvRef.ObstaclesRect[i].LengthMm / 2;
                        float Z1 = missionref.EnvRef.ObstaclesRect[i].PositionMm.Z - missionref.EnvRef.ObstaclesRect[i].WidthMm / 2;
                        float Z2 = missionref.EnvRef.ObstaclesRect[i].PositionMm.Z + missionref.EnvRef.ObstaclesRect[i].WidthMm / 2;

                        // �����굱ǰλ���Ƿ�λ���ϰ������������
                        if ((ptRealField.X > X1) && (ptRealField.X < X2) && (ptRealField.Y > Z1) && (ptRealField.Y < Z2))
                        {
                            if (!_isDragging && isForDragging) _isDragging = true;
                            _selectedRectObstacleId = i;    //isForDragging ? i : -1;  // ��i�������ϰ���
                            _selectedRoundObstacleId = -1;
                            _selectedChannelId = -1;
                            _selectedBallId = -1;
                            _selectedTeamId = -1;
                            _selectedFishId = -1;
                            isRectObstaclesSelected = true;
                            return; // �з�������ϰ��ﱻѡ���򲻿���������������ѡ��
                        }
                    }
                    else
                    {//��������ϰ���ķ���ǲ�Ϊ0
                        float deltaX = ptRealField.X - missionref.EnvRef.ObstaclesRect[i].PositionMm.X;
                        float deltaZ = ptRealField.Y - missionref.EnvRef.ObstaclesRect[i].PositionMm.Z;
                        double disToRectObstacle = Math.Sqrt(deltaX * deltaX + deltaZ * deltaZ);
                        float rad = Math.Min(missionref.EnvRef.ObstaclesRect[i].WidthMm, missionref.EnvRef.ObstaclesRect[i].LengthMm);

                        //�����굱ǰλ���Ƿ�λ���ϰ���������������Բ��
                        if (disToRectObstacle < rad)
                        {
                            if (!_isDragging && isForDragging) _isDragging = true;
                            _selectedRectObstacleId = i;    //isForDragging ? i : -1;  // ѡ�е�i����������ϰ���
                            _selectedRoundObstacleId = -1;    
                            _selectedChannelId = -1;
                            _selectedBallId = -1;
                            _selectedTeamId = -1;
                            _selectedFishId = -1;
                            isRectObstaclesSelected = true;
                            return; // �з�������ϰ��ﱻѡ�����򲻿���������������ѡ��
                        }
                    }//end else
                }//end  for (int i = 0...               
            }//end if (isRoundObstaclesSelected...

            #region
            ////û�з����ϰ��ﱻѡ�У�����Ƿ��з���ͨ����ѡ��
            //if (isRoundObstaclesSelected == false && isRectObstaclesSelected == false)
            //{
            //    for (int i = 0; i < missionref.EnvRef.Channels.Count; i++)
            //    {
            //        if (missionref.EnvRef.Channels[i].DirectionRad == 0)
            //        {//��������ϰ���ķ����Ϊ0
            //            float X1 = missionref.EnvRef.Channels[i].PositionMm.X - missionref.EnvRef.Channels[i].LengthMm / 2;
            //            float X2 = missionref.EnvRef.Channels[i].PositionMm.X + missionref.EnvRef.Channels[i].LengthMm / 2;
            //            float Z1 = missionref.EnvRef.Channels[i].PositionMm.Z - missionref.EnvRef.Channels[i].WidthMm / 2;
            //            float Z2 = missionref.EnvRef.Channels[i].PositionMm.Z + missionref.EnvRef.Channels[i].WidthMm / 2;

            //            // �����굱ǰλ���Ƿ�λ���ϰ������������
            //            if ((ptRealField.X > X1) && (ptRealField.X < X2) && (ptRealField.Y > Z1) && (ptRealField.Y < Z2))
            //            {
            //                if (!_isDragging && isForDragging) _isDragging = true;
            //                _selectedChannelId = i;    //isForDragging ? i : -1;  // ��i��ͨ��
            //                _selectedRoundObstacleId = -1;
            //                _selectedRectObstacleId = -1;
            //                _selectedBallId = -1;
            //                _selectedTeamId = -1;
            //                _selectedFishId = -1;
            //                isChannelsSelected = true;
            //                return;   // �з���ͨ����ѡ���򲻿���������������ѡ��
            //            }
            //        }//end if (missionref.EnvRef.Channels...
            //        else
            //        {//��������ϰ���ķ���ǲ�Ϊ0
            //            float deltaX = ptRealField.X - missionref.EnvRef.Channels[i].PositionMm.X;
            //            float deltaZ = ptRealField.Y - missionref.EnvRef.Channels[i].PositionMm.Z;
            //            double disToRectChannel = Math.Sqrt(deltaX * deltaX + deltaZ * deltaZ);
            //            float rad = Math.Min(missionref.EnvRef.Channels[i].WidthMm, missionref.EnvRef.Channels[i].LengthMm);

            //            //�����굱ǰλ���Ƿ�λ���ϰ���������������Բ��
            //            if (disToRectChannel < rad)
            //            {
            //                if (!_isDragging && isForDragging) _isDragging = true;
            //                _selectedChannelId = i;    //isForDragging ? i : -1;  // ѡ�е�i������ͨ��
            //                _selectedRoundObstacleId = -1;
            //                _selectedRectObstacleId = -1;
            //                _selectedBallId = -1;
            //                _selectedTeamId = -1;
            //                _selectedFishId = -1;
            //                isChannelsSelected = true;
            //                return;   // �з���ͨ����ѡ�����򲻿���������������ѡ��
            //            }
            //        }//end else
            //    }// end for (missionref...
            //}//end if (isRoundObstaclesSelected...
            #endregion
        }

        /// <summary>
        /// ����ƶ�ʱ��ʾ����ʵ�����꣬����ڷ����������߷���ˮ���ϣ�����ʾ����Ӧ��λ��
        /// </summary>
        private void picMatch_MouseMove(object sender, MouseEventArgs e)
        {
            // ������������ʱ��Ĭ��ѡ�з���ʹ����δ��ʼ�����ʱ�����
            if (MyMission.Instance().TeamsRef == null) return;

            MyMission missionref = MyMission.Instance();

            // ����ʹ����������ʱ�Ŵ�������ƶ���ʾʵ�ʳ������깦��
            if (!missionref.ParasRef.IsRunning)
            {
                Point ptRealField = Field.Instance().PixToMm(new Point(e.X, e.Y));
                bool isBallSelected = false;
                bool isFishSelected = false;
                bool isRoundObstaclesSelected = false;
                bool isRectObstaclesSelected = false;
                bool isChannelsSelected = false;
                DetectRoboFishAndBall(ptRealField, false, ref isBallSelected, ref isFishSelected);
                DetectObstaclesAndChannels(ptRealField, false, ref isRoundObstaclesSelected, ref isRectObstaclesSelected, ref isChannelsSelected);
                
                if (isBallSelected == true)         // ��껬������ˮ��
                {
                    // ��ǰ����ʹ��ֻʹ����һ������ˮ��ʱ��ѡ�к�ֻ��ʾBall��������ʾBall1,Ball2...
                    lblFishOrBallSelected.Text = "Selected: ";
                    lblFishOrBallSelected.Text += 
                        (missionref.EnvRef.Balls.Count > 1) ? "Ball" + (_selectedBallId + 1) : "Ball";
                }
                else if (isFishSelected == true)    // ��껬�����������
                {
                    lblFishOrBallSelected.Text =
                        String.Format("Selected: Team{0} Fish{1}", _selectedTeamId + 1, _selectedFishId + 1);
                }
                else if (isRoundObstaclesSelected == true)  //��껬������Բ���ϰ���
                {
                    // ��ǰ����ʹ��ֻʹ����һ������Բ���ϰ���ʱ��ѡ�к�ֻ��ʾRoundObstacle��������ʾRoundObstacle1,RoundObstacle2...
                    lblFishOrBallSelected.Text = "Selected: ";
                    lblFishOrBallSelected.Text +=
                        (missionref.EnvRef.ObstaclesRound.Count > 1) ? "RoundObstacle" + (_selectedRoundObstacleId + 1) : "RoundObstacle";
                }
                else if (isRectObstaclesSelected == true)  //��껬����������ϰ���
                {
                    // ��ǰ����ʹ��ֻʹ����һ����������ϰ���ʱ��ѡ�к�ֻ��ʾRectObstacle��������ʾRectObstacle1,RectObstacle2...
                    lblFishOrBallSelected.Text = "Selected: ";
                    lblFishOrBallSelected.Text +=
                        (missionref.EnvRef.ObstaclesRect.Count > 1) ? "RectObstacle" + (_selectedRectObstacleId + 1) : "RectObstacle";
                }
                else if (isChannelsSelected == true)  //��껬������ͨ��
                {
                    // ��ǰ����ʹ��ֻʹ����һ����������ϰ���ʱ��ѡ�к�ֻ��ʾChannel��������ʾChannel1,Channel2...
                    lblFishOrBallSelected.Text = "Selected: ";
                    lblFishOrBallSelected.Text +=
                        (missionref.EnvRef.Channels.Count > 1) ? "Channel" + (_selectedChannelId + 1) : "Channel";
                }
                else
                {
                    lblFishOrBallSelected.Text = "";
                }

                lblFishOrBallPosition.Text = "X: " + ptRealField.X + " Z: " + ptRealField.Y;
                this.Cursor = (isBallSelected || isFishSelected || isRoundObstaclesSelected || isRectObstaclesSelected || isChannelsSelected) ? Cursors.NoMove2D : Cursors.Default;

                //picMatch.Image = MyMission.Instance().IMissionRef.Draw();
                SetMatchBmp(MyMission.Instance().IMissionRef.Draw());
            }
        }

        /// <summary>
        /// ����뿪���ػ�ͼ����PictureBoxʱ�������ʾ�����������Ϣ
        /// </summary>
        private void picMatch_MouseLeave(object sender, EventArgs e)
        {
            lblFishOrBallPosition.Text = "";
            //picMatch.Image = MyMission.Instance().IMissionRef.Draw();
            SetMatchBmp(MyMission.Instance().IMissionRef.Draw());
        }

        #region ����϶����泡���ϵĶ�̬���󣨷��������/ˮ��/�ϰ��
        /// <summary>
        /// ��ǰ���泡����ѡ�еķ�����������ڶ����ţ���3V3�е�0,1��
        /// </summary>
        int _selectedTeamId = -1;

        /// <summary>
        /// ��ǰ���泡����ѡ�еķ���������������ڶ�����ڵı�ţ���3V3�е�0,1,2��
        /// </summary>
        int _selectedFishId = -1;

        /// <summary>
        /// ��ǰ���泡����ѡ�еķ���ˮ����Env.Balls�б��еı�ţ���3V3�е�0��
        /// </summary>
        int _selectedBallId = -1;

        /// <summary>
        /// ��ǰ���泡����ѡ�еķ���Բ���ϰ�����Env.ObstaclesRounds�б��еı��
        /// </summary>
        int _selectedRoundObstacleId = -1;

        /// <summary>
        /// ��ǰ���泡����ѡ�еķ��淽���ϰ�����Env.ObstaclesRects�б��еı��
        /// </summary>
        int _selectedRectObstacleId = -1;

        /// <summary>
        /// ��ǰ���泡����ѡ�еķ���ͨ����Env.Channels�б��еı��
        /// </summary>
        int _selectedChannelId = -1;

        /// <summary>
        /// ָʾ��ǰ�Ƿ�����ִ���϶�����
        /// </summary>
        bool _isDragging = false;

        /// <summary>
        /// ��갴��ʱ׼���϶������������߷���ˮ��
        /// </summary>
        private void picMatch_MouseDown(object sender, MouseEventArgs e)
        {
            // ������������ʱ��Ĭ��ѡ�з���ʹ����δ��ʼ�����ʱ�����
            if (MyMission.Instance().TeamsRef == null) return;
            // ����ʹ����������ʱ�������϶����ϵĶ�̬���󣨷��������ͷ���ˮ��
            //if (!MyMission.Instance().ParasRef.IsRunning)
            //{
            Point ptRealField = Field.Instance().PixToMm(new Point(e.X, e.Y));
            bool isBallSelected = false;
            bool isFishSelected = false;
            bool isRoundObstaclesSelected = false;
            bool isRectObstaclesSelected = false;
            bool isChannelsSelected = false;

            DetectRoboFishAndBall(ptRealField, true, ref isBallSelected, ref isFishSelected);
            DetectObstaclesAndChannels(ptRealField, true, ref isRoundObstaclesSelected, ref isRectObstaclesSelected, ref isChannelsSelected);    

            this.Cursor = (isBallSelected || isFishSelected || isRoundObstaclesSelected || isRectObstaclesSelected || isChannelsSelected) ? Cursors.NoMove2D : Cursors.Default;
            //}
        }

        /// <summary>
        /// ����ͷ�ʱ�����������/ˮ��/�ϰ���ŵ����ĵ�ǰλ��
        /// </summary>
        private void picMatch_MouseUp(object sender, MouseEventArgs e)
        {
            // ����ʹ����������ʱ�������϶����ϵĶ�̬���󣨷��������ͷ���ˮ��
            //if (!MyMission.Instance().ParasRef.IsRunning)
            //{
            // �����϶���ʲô��ûѡ����ֱ�ӷ���
            if ((_isDragging == false) || 
                ((_selectedBallId == -1) && (_selectedTeamId == -1) && (_selectedFishId == -1) 
                && (_selectedRectObstacleId == -1) && (_selectedRoundObstacleId == -1))) return;

            MyMission myMission = MyMission.Instance();
            Field f = Field.Instance();
            Point ptRealField = Field.Instance().PixToMm(new Point(e.X, e.Y));

            if (myMission.ParasRef.IsGoalBlockNeeded)//��Ҫ�������ſ�ʱ�ĳ������ұ߽��ⷽ�� added by zhangbo 20111110
            {
                if ((ptRealField.X < f.LeftMm) && (ptRealField.Y > -f.GoalWidthMm / 2)
                    && (ptRealField.Y < f.GoalWidthMm / 2))
                {
                    ptRealField.X = f.LeftMm;
                }
                else if ((ptRealField.X < f.LeftMm + f.GoalDepthMm)
                    && ((ptRealField.Y < -f.GoalWidthMm / 2) || (ptRealField.Y > f.GoalWidthMm / 2)))
                {
                    ptRealField.X = f.LeftMm + f.GoalDepthMm;
                }
                else if ((ptRealField.X > f.RightMm) && (ptRealField.Y > -f.GoalWidthMm / 2)
                    && (ptRealField.Y < f.GoalWidthMm / 2))
                {
                    ptRealField.X = f.RightMm;
                }
                else if ((ptRealField.X > f.RightMm - f.GoalDepthMm)
                    && ((ptRealField.Y < -f.GoalWidthMm / 2) || (ptRealField.Y > f.GoalWidthMm / 2)))
                {
                    ptRealField.X = f.RightMm - f.GoalDepthMm;
                }
            }
            else//��ʱ������û�����ſ� added by zhangbo 20111110
            {
                if (ptRealField.X < f.LeftMm)
                {
                    ptRealField.X = f.LeftMm;
                }
                else if (ptRealField.X > f.RightMm)
                {
                    ptRealField.X = f.RightMm;
                }
            }

            if (ptRealField.Y < f.TopMm)
            {
                ptRealField.Y = f.TopMm;
            }
            else if (ptRealField.Y > f.BottomMm)
            {
                ptRealField.Y = f.BottomMm;
            }

            if (_isDragging && (_selectedBallId > -1))
            {// ѡ�е��Ƿ���ˮ��
                myMission.EnvRef.Balls[_selectedBallId].PositionMm.X = ptRealField.X;
                myMission.EnvRef.Balls[_selectedBallId].PositionMm.Z = ptRealField.Y;

                // �����Ϸ��Լ�鼰����
                Ball obj = myMission.EnvRef.Balls[_selectedBallId];
                UrwpgSimHelper.ParametersCheckingBall(ref obj);

                // ������ʾ
                _listBallSettingControls[_selectedBallId].TxtPositionMmX.Text = obj.PositionMm.X.ToString();
                _listBallSettingControls[_selectedBallId].TxtPositionMmZ.Text = obj.PositionMm.Z.ToString();

                _selectedBallId = -1;
            }
            else if (_isDragging && (_selectedTeamId > -1) && (_selectedFishId > -1))
            { // ѡ�е��Ƿ��������
                myMission.TeamsRef[_selectedTeamId].Fishes[_selectedFishId].PositionMm.X = ptRealField.X;
                myMission.TeamsRef[_selectedTeamId].Fishes[_selectedFishId].PositionMm.Z = ptRealField.Y;

                // �����Ϸ��Լ�鼰����
                RoboFish obj = myMission.TeamsRef[_selectedTeamId].Fishes[_selectedFishId];
                UrwpgSimHelper.ParametersCheckingRoboFish(ref obj);

                // ������ʾ
                _listFishSettingControls[_selectedTeamId].CmbPlayers.SelectedIndex = -1;
                _listFishSettingControls[_selectedTeamId].CmbPlayers.SelectedIndex = _selectedFishId;

                _selectedTeamId = -1;
                _selectedFishId = -1;
            }
            else if (_isDragging && (_selectedRoundObstacleId > -1))
            {//ѡ�е��Ƿ���Բ���ϰ���
                if (myMission.ParasRef.IsRunning == false)
                {// ����ʹ���������в������϶������ϰ���
                    myMission.EnvRef.ObstaclesRound[_selectedRoundObstacleId].PositionMm.X = ptRealField.X;
                    myMission.EnvRef.ObstaclesRound[_selectedRoundObstacleId].PositionMm.Z = ptRealField.Y;

                    // �����Ϸ��Լ�鼰����
                    RoundedObstacle obj = myMission.EnvRef.ObstaclesRound[_selectedRoundObstacleId];
                    UrwpgSimHelper.ParametersCheckingObstacle(ref obj);

                    // ������ʾ                    
                    cmbObastacleType.SelectedIndex = 0; // ѡ�С�CIRCLE��
                    lstObstacle.SelectedIndex = -1;
                    lstObstacle.SelectedIndex = _selectedRoundObstacleId;

                    _selectedRoundObstacleId = -1;
                }
            }
            else if (_isDragging && (_selectedRectObstacleId > -1))
            {//ѡ�е��Ƿ�������ϰ���
                if (myMission.ParasRef.IsRunning == false)
                {// ����ʹ���������в������϶������ϰ���
                    myMission.EnvRef.ObstaclesRect[_selectedRectObstacleId].PositionMm.X = ptRealField.X;
                    myMission.EnvRef.ObstaclesRect[_selectedRectObstacleId].PositionMm.Z = ptRealField.Y;

                    // �����Ϸ��Լ�鼰����
                    RectangularObstacle obj = myMission.EnvRef.ObstaclesRect[_selectedRectObstacleId];
                    UrwpgSimHelper.ParametersCheckingObstacle(ref obj);

                    // ������ʾ
                    cmbObastacleType.SelectedIndex = 1; // ѡ�С�RECTANGLE��
                    lstObstacle.SelectedIndex = -1;
                    lstObstacle.SelectedIndex = _selectedRectObstacleId;

                    _selectedRectObstacleId = -1;
                }
            }

            //picMatch.Image = myMission.IMissionRef.Draw();
            SetMatchBmp(myMission.IMissionRef.Draw());

            _isDragging = false;
            this.Cursor = Cursors.Default;
        }
        #endregion
        #endregion

        #region ���泡�ػ��ƿؼ�PictureBox��Paint/DoubleClick�¼���������Form��KeyDown�¼� Weiqingdan
        /// <summary>
        /// ��Ӧ��ͼ�ؼ�PictureBox��Paint�¼� ʵ��ˮ�������Label��͸����ʾ
        /// </summary>
        private void picMatch_Paint(object sender, PaintEventArgs e)
        {
            Field f = Field.Instance();
            MyMission myMission = MyMission.Instance();
            if (myMission.TeamsRef == null) return;
            SolidBrush brush = new SolidBrush(lblTmp.ForeColor);    // ʹ������/������ɫ
            SolidBrush brushEmphasized = new SolidBrush(Color.Red); // ����ʱ/�ȷ���ɫ

            // ʹ�����ƺ͵���ʱ��ʾ�о������������10���ؾ��볡���±߾�80����
            lblTmp.Location = new Point(f.FieldOuterBorderPix + f.FieldInnerBorderPix + f.GoalDepthPix + 10,
                2 * f.FieldOuterBorderPix + 2 * f.FieldInnerBorderPix + f.FieldLengthZPix - 80);

            int startX = lblTmp.Location.X;         // ʹ�����ƺ͵���ʱ��ʾ��X����
            lblTmp.Text = myMission.ParasRef.Name;  // ��ʾʹ������
            e.Graphics.DrawString(lblTmp.Text, lblTmp.Font, brush, startX, lblTmp.Location.Y);

            startX += lblTmp.Width + 10;            // X��������ƫ��ʹ��������ռ����ֵ
            int seconds = myMission.ParasRef.RemainingCycles * myMission.ParasRef.MsPerCycle / 1000;
            lblTmp.Text = string.Format("{0:00} : {1:00}", seconds / 60, seconds % 60); // ��ʾʹ������ʱ
            e.Graphics.DrawString(lblTmp.Text, lblTmp.Font, brushEmphasized, startX, lblTmp.Location.Y);

            lblTmp.Location = new Point(lblTmp.Location.X, lblTmp.Location.Y + 30);
            startX = lblTmp.Location.X;         // �������ƺͼƷ���ʾ��X����

            // 2֧����ĶԿ��Ա����ԡ�����1 �÷�1 : �÷�2 ����2����ʽ��ʾ
            if (myMission.TeamsRef.Count == 2)
            {
                lblTmp.Text = myMission.TeamsRef[0].Para.Name; // ��ʾ����1
                e.Graphics.DrawString(lblTmp.Text, lblTmp.Font, brush, startX, lblTmp.Location.Y);

                startX += lblTmp.Width + 10;
                lblTmp.Text = string.Format("{0:00} : {1:00}", myMission.TeamsRef[0].Para.Score,
                    myMission.TeamsRef[1].Para.Score); // �Աȷ���ʽ��ʾ2֧����ĵ÷�
                e.Graphics.DrawString(lblTmp.Text, lblTmp.Font, brushEmphasized, startX, lblTmp.Location.Y);

                startX += lblTmp.Width + 10;
                lblTmp.Text = myMission.TeamsRef[1].Para.Name; // ��ʾ����2
                e.Graphics.DrawString(lblTmp.Text, lblTmp.Font, brush, startX, lblTmp.Location.Y);
            }
            //else if (myMission.TeamsRef.Count == 1)    // 1֧����ֻ��ʾ����
            //{
            //    lblTmp.Text = myMission.TeamsRef[0].Para.Name;
            //    e.Graphics.DrawString(lblTmp.Text, lblTmp.Font, brush, startX, lblTmp.Location.Y);
            //}
            else // 3֧�����ϵĶ��鰴����ʾ������ �÷֡�
            {
                startX -= 10;                           // ����ʹ��1֧��������ǰ����Ҫ��ƫ��

                // ������ʾ������i �÷�i����Ϣ
                for (int i = 0; i < myMission.TeamsRef.Count; i++)
                {
                    startX += 10;                       // �����һ�顰���� �÷֡�ƫ��10����
                    lblTmp.Text = myMission.TeamsRef[i].Para.Name;
                    e.Graphics.DrawString(lblTmp.Text, lblTmp.Font, brush, startX, lblTmp.Location.Y);

                    if (myMission.TeamsRef[i].Para.Score >= 0)
                    {// �÷�Ϊ��ֵ����ʾ ���Կ��Ը�����Ҫ�õ��÷�ֵ�ķ���ʹ���Ķ���÷�ֵ��Ϊ��ֵ
                        startX += lblTmp.Width + 10;        // �÷���Զ���ƫ��10����
                        lblTmp.Text = string.Format("{0:00}", myMission.TeamsRef[0].Para.Score);
                        e.Graphics.DrawString(lblTmp.Text, lblTmp.Font, brushEmphasized, startX, lblTmp.Location.Y);
                        startX += lblTmp.Width;
                    }
                }
            }

            // ���Ƶײ�״̬���еĵ�ǰ��������
            Label LB_S = (Label)lblFishOrBallPosition;
            LB_S.Visible = false;
            e.Graphics.DrawString(LB_S.Text, LB_S.Font, new SolidBrush(LB_S.ForeColor),
                f.FieldOuterBorderPix + f.FieldInnerBorderPix + f.GoalDepthPix + 10,
                2 * f.FieldOuterBorderPix + 2 * f.FieldInnerBorderPix + f.FieldLengthZPix - 18);

            // ���Ƶײ�״̬���еĵ�ǰѡ�ж��󣨷���ˮ��/���������/�����ϰ���ȣ�����
            Label lbl_S = (Label)lblFishOrBallSelected;
            lbl_S.Visible = false;
            e.Graphics.DrawString(lbl_S.Text, lbl_S.Font, new SolidBrush(lbl_S.ForeColor),
                f.FieldOuterBorderPix + f.FieldInnerBorderPix + f.GoalDepthPix + 10 + LB_S.Size.Width,
                2 * f.FieldOuterBorderPix + 2 * f.FieldInnerBorderPix + f.FieldLengthZPix - 18);

            SolidBrush drawBrushA = new SolidBrush(Color.Red);
            SolidBrush drawBrushB = new SolidBrush(Color.Yellow);
            if (myMission.ParasRef.TeamCount == 2)
            {// ����2֧�������ķ���ʹ���������Ұ볡��־��ɫ���������������ǰ��ɫ����ɫһ��
                drawBrushA = new SolidBrush(myMission.TeamsRef[0].Fishes[0].ColorFish);
                drawBrushB = new SolidBrush(myMission.TeamsRef[1].Fishes[0].ColorFish);
            }
            // ������볡��־
            String drawStringA = "L";
            Font drawFontA = new Font("����", 20);
            PointF drawPointA = new PointF(0, (f.FieldLengthZPix - f.GoalWidthPix) / 2);
            e.Graphics.DrawString(drawStringA, drawFontA, drawBrushA, drawPointA);
            e.Graphics.FillRectangle(drawBrushA,
                new Rectangle(new Point(3, (f.FieldLengthZPix + f.GoalWidthPix) / 2), new Size(14, 14)));

            // �����Ұ볡��־
            String drawStringB = "R";
            Font drawFontB = new Font("����", 20);
            PointF drawPointB = new PointF(f.FieldLengthXPix + 2 * f.FieldInnerBorderPix + 20,
                (f.FieldLengthZPix - f.GoalWidthPix) / 2);
            e.Graphics.DrawString(drawStringB, drawFontB, drawBrushB, drawPointB);
            e.Graphics.FillRectangle(drawBrushB,
                new Rectangle(new Point(f.FieldLengthXPix + 2 * f.FieldInnerBorderPix + 2 * f.FieldOuterBorderPix - 17,
                    (f.FieldLengthZPix + f.GoalWidthPix) / 2), new Size(14, 14)));
        }

        // Weiqingdan
        /// <summary>
        /// ��Ӧ��ͼ�ؼ�PictureBox��DoubleClick�¼� ������˳�ȫ��ģʽ
        /// </summary>
        private void picMatch_DoubleClick(object sender, EventArgs e)
        {
            #region
            // ˮ�������ʼ��
            InitWaveEffect();
            Field f = Field.Instance();

            // ȫ��������Ӧ����ʾ������ LiYoubing 20110803
            // ���ڵ�ǰ������ʾ��
            Screen currentScreen = Screen.FromHandle(this.Handle);
            int ScreenWidth = currentScreen.Bounds.Width;    // ��ʾ�����
            int ScreenHeight = currentScreen.Bounds.Height;  // ��ʾ���߶�
            if (this.WindowState == FormWindowState.Normal)     // ��ǰ����������״̬��Ŵ�ȫ��
            {
                this.FormBorderStyle = FormBorderStyle.None;    // ��ʱthis.FormBorderStyleΪNone ������ʾ��������������
                this.WindowState = FormWindowState.Maximized;
                //this.TopMost = true;

                //int ScreenWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;    // ��ʾ�����
                //int ScreenHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;  // ��ʾ���߶�
                double dRatioScreenWidthAndHeight = (double)ScreenWidth / (double)ScreenHeight;

                double dRatioFieldXMmAndZMm = (double)(f.FieldLengthXMm) / (double)(f.FieldLengthZMm);
                if (dRatioFieldXMmAndZMm <= dRatioScreenWidthAndHeight)
                {
                    f.FieldLengthZPix = ScreenHeight - 2 * f.FieldInnerBorderPix - 2 * f.FieldOuterBorderPix;
                    f.ScaleMmToPix = (double)f.FieldLengthZMm / (double)f.FieldLengthZPix;
                    f.FieldLengthXPix = (int)(f.FieldLengthXMm / f.ScaleMmToPix + 0.5);
                    f.GoalDepthPix = (int)(f.GoalDepthMm / f.ScaleMmToPix + 0.5);
                    f.GoalWidthPix = (int)(f.GoalWidthMm / f.ScaleMmToPix + 0.5);
                    f.GoalBlockWidthPix = (int)((f.FieldLengthZPix - f.GoalWidthPix) / 2 + 0.5);
                    f.GoalBlockDepthPix = f.GoalDepthPix;

                    f.PictureBoxXPix = f.FieldLengthXPix + 2 * f.FieldInnerBorderPix + 2 * f.FieldOuterBorderPix;
                    f.PictureBoxZPix = f.FieldLengthZPix + 2 * f.FieldInnerBorderPix + 2 * f.FieldOuterBorderPix;
                    f.ForbiddenZoneLengthXPix = (int)(f.ForbiddenZoneLengthXMm / f.ScaleMmToPix + 0.5);
                    f.ForbiddenZoneLengthZPix = (int)(f.ForbiddenZoneLengthZMm / f.ScaleMmToPix + 0.5);
                    f.CenterCircleRadiusPix = (int)(f.CenterCircleRadiusMm / f.ScaleMmToPix + 0.5);

                    int iSubtractScreenWidthAndPictureBoxX = ScreenWidth - f.PictureBoxXPix;
                    picMatch.Location = new Point(iSubtractScreenWidthAndPictureBoxX / 2, 0);
                    picMatch.Width = f.PictureBoxXPix;
                    picMatch.Height = f.PictureBoxZPix;

                    tabServerControlBoard.Visible = false;
                }
                else
                {
                    f.PictureBoxXPix = ScreenWidth;
                    f.FieldLengthXPix = f.PictureBoxXPix - 2 * f.FieldOuterBorderPix - 2 * f.FieldInnerBorderPix;
                    f.ScaleMmToPix = (double)(f.FieldLengthXMm) / (double)(f.FieldLengthXPix);
                    f.FieldLengthZPix = (int)(f.FieldLengthZMm / f.ScaleMmToPix + 0.5);
                    f.GoalDepthPix = (int)(f.GoalDepthMm / f.ScaleMmToPix + 0.5);
                    f.GoalWidthPix = (int)(f.GoalWidthMm / f.ScaleMmToPix + 0.5);
                    f.GoalBlockWidthPix = (int)((f.FieldLengthZPix - f.GoalWidthPix) / 2 + 0.5);
                    f.GoalBlockDepthPix = f.GoalDepthPix;
                    f.PictureBoxZPix = f.FieldLengthZPix + 2 * f.FieldInnerBorderPix + 2 * f.FieldOuterBorderPix;

                    f.ForbiddenZoneLengthXPix = (int)(f.ForbiddenZoneLengthXMm / f.ScaleMmToPix + 0.5);
                    f.ForbiddenZoneLengthZPix = (int)(f.ForbiddenZoneLengthZMm / f.ScaleMmToPix + 0.5);
                    f.CenterCircleRadiusPix = (int)(f.CenterCircleRadiusMm / f.ScaleMmToPix + 0.5);

                    int iSubtractScreenHeightAndPictureBoxZ = ScreenHeight - f.PictureBoxZPix;
                    picMatch.Location = new Point(0, iSubtractScreenHeightAndPictureBoxZ / 2);
                    picMatch.Width = f.PictureBoxXPix;
                    picMatch.Height = f.PictureBoxZPix;

                    tabServerControlBoard.Visible = false;
                }
            }
            else // ��ǰ��ȫ��״̬��ָ�����������״̬
            {
                //int ScreenWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;    // ��ʾ�����
                //int ScreenHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;  // ��ʾ���߶� 
                double dRatioPictureBoxWidthAndHeight = (double)(ScreenWidth - f.TabControlLengthXPix - 2 * f.FormPaddingPix
                    - f.SpanBetweenFieldAndTabControlPix - 10) / (double)(f.TabControlLengthZPix);
                double dRatioFieldXMmAndZMm = (double)(f.FieldLengthXMm) / (double)(f.FieldLengthZMm);
                if (dRatioFieldXMmAndZMm <= dRatioPictureBoxWidthAndHeight)
                {
                    this.FormBorderStyle = FormBorderStyle.Sizable;
                    this.WindowState = FormWindowState.Normal;

                    f.FieldLengthZPix = f.TabControlLengthZPix - 2 * f.FieldInnerBorderPix - 2 * f.FieldOuterBorderPix;
                    f.ScaleMmToPix = (double)f.FieldLengthZMm / (double)f.FieldLengthZPix;
                    f.FieldLengthXPix = (int)(f.FieldLengthXMm / f.ScaleMmToPix + 0.5);
                    f.GoalDepthPix = (int)(f.GoalDepthMm / f.ScaleMmToPix + 0.5);
                    f.GoalWidthPix = (int)(f.GoalWidthMm / f.ScaleMmToPix + 0.5);
                    f.GoalBlockWidthPix = (int)((f.FieldLengthZPix - f.GoalWidthPix) / 2 + 0.5);
                    f.GoalBlockDepthPix = f.GoalDepthPix;

                    f.PictureBoxXPix = f.FieldLengthXPix + 2 * f.FieldInnerBorderPix + 2 * f.FieldOuterBorderPix;
                    f.PictureBoxZPix = f.FieldLengthZPix + 2 * f.FieldInnerBorderPix + 2 * f.FieldOuterBorderPix;
                    f.FormLengthXPix = f.TabControlLengthXPix + f.FieldLengthXPix + f.SpanBetweenFieldAndTabControlPix
                        + 2 * f.FieldInnerBorderPix + 2 * f.FieldOuterBorderPix + 2 * f.FormPaddingPix;
                    f.FormLengthZPix = f.TabControlLengthZPix + 2 * f.FormPaddingPix;
                    f.ForbiddenZoneLengthXPix = (int)(f.ForbiddenZoneLengthXMm / f.ScaleMmToPix + 0.5);
                    f.ForbiddenZoneLengthZPix = (int)(f.ForbiddenZoneLengthZMm / f.ScaleMmToPix + 0.5);
                    f.CenterCircleRadiusPix = (int)(f.CenterCircleRadiusMm / f.ScaleMmToPix + 0.5);

                    picMatch.Location = new Point(10, 10);
                }
                else
                {
                    this.FormBorderStyle = FormBorderStyle.Sizable;
                    this.WindowState = FormWindowState.Normal;

                    f.PictureBoxXPix = ScreenWidth - f.TabControlLengthXPix - 2 * f.FormPaddingPix - f.SpanBetweenFieldAndTabControlPix - 10;
                    f.FieldLengthXPix = f.PictureBoxXPix - 2 * f.FieldOuterBorderPix - 2 * f.FieldInnerBorderPix;
                    f.ScaleMmToPix = (double)(f.FieldLengthXMm) / (double)(f.FieldLengthXPix);
                    f.FieldLengthZPix = (int)(f.FieldLengthZMm / f.ScaleMmToPix + 0.5);
                    f.GoalDepthPix = (int)(f.GoalDepthMm / f.ScaleMmToPix + 0.5);
                    f.GoalWidthPix = (int)(f.GoalWidthMm / f.ScaleMmToPix + 0.5);
                    f.GoalBlockWidthPix = (int)((f.FieldLengthZPix - f.GoalWidthPix) / 2 + 0.5);
                    f.GoalBlockDepthPix = f.GoalDepthPix;
                    f.PictureBoxZPix = f.FieldLengthZPix + 2 * f.FieldInnerBorderPix + 2 * f.FieldOuterBorderPix;

                    f.FormLengthXPix = f.TabControlLengthXPix + f.FieldLengthXPix + f.SpanBetweenFieldAndTabControlPix
                        + 2 * f.FieldInnerBorderPix + 2 * f.FieldOuterBorderPix + 2 * f.FormPaddingPix;
                    f.FormLengthZPix = f.TabControlLengthZPix + 2 * f.FormPaddingPix;
                    f.ForbiddenZoneLengthXPix = (int)(f.ForbiddenZoneLengthXMm / f.ScaleMmToPix + 0.5);
                    f.ForbiddenZoneLengthZPix = (int)(f.ForbiddenZoneLengthZMm / f.ScaleMmToPix + 0.5);
                    f.CenterCircleRadiusPix = (int)(f.CenterCircleRadiusMm / f.ScaleMmToPix + 0.5);

                    int iSubtractFormLengthZPixAndPictureBoxZPix = f.FormLengthZPix - f.PictureBoxZPix;
                    picMatch.Location = new Point(10, iSubtractFormLengthZPixAndPictureBoxZPix / 2);
                }
                // ���������ڳߴ� LiYoubing 20110810
                this.ClientSize = new Size(f.FormLengthXPix, f.FormLengthZPix);
                tabServerControlBoard.Visible = true;
                // ���ÿ������λ�� LiYoubing 20110810
                tabServerControlBoard.Location = new Point(f.FormPaddingPix + 2 * f.FieldOuterBorderPix 
                    + 2 * f.FieldInnerBorderPix + f.FieldLengthXPix + f.SpanBetweenFieldAndTabControlPix, 
                    f.FormPaddingPix);
                //this.TopMost = false;
            }

            f.ScaleMmToPixX = f.ScaleMmToPix;
            f.ScaleMmToPixZ = f.ScaleMmToPix;
            f.FieldCenterXPix = f.FieldOuterBorderPix + f.FieldInnerBorderPix + f.FieldLengthXPix / 2;
            f.FieldCenterZPix = f.FieldOuterBorderPix + f.FieldInnerBorderPix + f.FieldLengthZPix / 2;

            picMatch.Width = f.PictureBoxXPix;
            picMatch.Height = f.PictureBoxZPix;

            if (picMatch.BackgroundImage != null)
            {
                picMatch.BackgroundImage.Dispose();
            }
            Bitmap bmp = URWPGSim2D.Gadget.WaveEffect.Instance().GetBitmap();
            picMatch.BackgroundImage = f.Draw(bmp, MyMission.Instance().ParasRef.IsGoalBlockNeeded, MyMission.Instance().ParasRef.IsFieldInnerLinesNeeded);
            //picMatch.Image = MyMission.Instance().IMissionRef.Draw();
            SetMatchBmp(MyMission.Instance().IMissionRef.Draw());
            #endregion
        }

        // Weiqingdan
        /// <summary>
        /// ��Ӧ��������ȼ�ESC/Alt+P/Alt+C/AltS/Alt+R/Alt+Y/Alt+E/Alt+I/Alt+V
        /// </summary>
        private void ServerControlBoard_KeyDown(object sender, KeyEventArgs e)
        {
            MyMission myMission = MyMission.Instance();
            if (this.WindowState == FormWindowState.Maximized)
            {// ȫ��ģʽ
                if (e.KeyCode == Keys.Escape)
                {// ��ӦESC���˳�ȫ��ģʽ
                    picMatch_DoubleClick(this, null);
                }
            }
            if (e.KeyCode == Keys.P && e.Modifiers == Keys.Alt 
                && btnPause.Enabled == true && myMission.ParasRef.IsRunning == true
                && myMission.ParasRef.IsPaused == false)
            {// Pause/Continue��ť����ʱ��ӦAlt+P�ȼ�
                btnPause_Click(null, null);
            }
            else if (e.KeyCode == Keys.C && e.Modifiers == Keys.Alt
                && btnPause.Enabled == true && myMission.ParasRef.IsRunning == true
                && myMission.ParasRef.IsPaused == true)
            {// Pause/Continue��ť����ʱ��ӦAlt+C�ȼ�
                btnPause_Click(null, null);
            }
            else if (e.KeyCode == Keys.S && e.Modifiers == Keys.Alt && btnStart.Enabled == true)
            {// Start��ť����ʱ��ӦAlt+S�ȼ�
                btnStart_Click(null, null);
            }
            else if (e.KeyCode == Keys.R && e.Modifiers == Keys.Alt && btnRestart.Enabled == true)
            {// Restart��ť����ʱ��ӦAlt+R�ȼ�
                btnRestart_Click(null, null);
            }
            else if (e.KeyCode == Keys.Y && e.Modifiers == Keys.Alt 
                && btnReplay.Enabled == true && _isReplaying == false)
            {// Replay/End��ť����ʱ��ӦAlt+Y�ȼ�
                btnReplay_Click(null, null);
            }
            else if (e.KeyCode == Keys.E && e.Modifiers == Keys.Alt 
                && btnReplay.Enabled == true && _isReplaying == true)
            {// Replay/End��ť����ʱ��ӦAlt+E�ȼ�
                btnReplay_Click(null, null);
            }
            else if (e.KeyCode == Keys.I && e.Modifiers == Keys.Alt
                && btnCapture.Enabled == true)
            {// Image��ť����ʱ��ӦAlt+I�ȼ�
                btnCapture_Click(null, null);
            }
            else if (e.KeyCode == Keys.V && e.Modifiers == Keys.Alt
                && btnRecordVideo.Enabled == true)
            {// Video��ť����ʱ��ӦAlt+V�ȼ�
                btnRecordVideo_Click(null, null);
            }
            //else if (e.KeyCode == Keys.R && e.Alt && e.Control
            //    && btnReplay.Enabled == true && btnReplay.Text == "Replay(R)")
            //{// Replay/End��ť����ʱ��ӦCtrl+Alt+R�ȼ�
            //    btnReplay_Click(null, null);
            //}
            //else if (e.KeyCode == Keys.E && e.Alt && e.Control
            //    && btnReplay.Enabled == true && btnReplay.Text == "End(E)")
            //{// Replay/End��ť����ʱ��ӦCtrl+Alt+E�ȼ�
            //    btnReplay_Click(null, null);
            //}
        }
        #endregion

        #region ��ͼ����
        /// <summary>
        /// ��ͼ���ܣ�����API����
        /// </summary>
        /// <param name="hdcDest">Ŀ���豸�ľ��</param>
        /// <param name="nXDest">Ŀ���������Ͻǵ�X����</param>
        /// <param name="nYDest">Ŀ���������Ͻǵ�Y����</param>
        /// <param name="nWidth">Ŀ�����ľ��εĿ��</param>
        /// <param name="nHeight">Ŀ�����ľ��εĳ���(�߶�)</param>
        /// <param name="hdcSrc">Դ�豸�ľ��</param>
        /// <param name="nXSrc">Դ��������Ͻǵ�X����</param>
        /// <param name="nYSrc">Դ��������Ͻǵ�Y����</param>
        /// <param name="dwRop">��դ�Ĳ���ֵ</param>
        /// <returns></returns>
        [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, System.Int32 dwRop);

        /// <summary>
        /// ��ͼ���ܣ�����API����
        /// </summary>
        /// <param name="lpszDriver">��������</param>
        /// <param name="lpszDevice">�豸����</param>
        /// <param name="lpszOutput">���ã�������Ϊ"NULL"</param>
        /// <param name="lpInitData">����Ĵ�ӡ������</param>
        /// <returns></returns>
        [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
        private static extern IntPtr CreateDC(string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData);

        /// <summary>
        /// ��ͼ���ܣ���ͼ��ť����¼�
        /// </summary>
        private void btnCapture_Click(object sender, EventArgs e)
        {
            try
            {
                //IntPtr dc1 = ServerControlBoard.CreateDC("DISPLAY", null, null, (IntPtr)null);
                //Graphics g1 = Graphics.FromHdc(dc1); //��һ��ָ���豸�ľ������һ���µ�Graphics����
                //Bitmap MyImage = new Bitmap(this.Width, this.Height, g1); //������Ļ��С����һ����֮��ͬ��С��Bitmap����
                //Graphics g2 = Graphics.FromImage(MyImage); //�����Ļ�ľ��
                //IntPtr dc3 = g1.GetHdc(); //���λͼ�ľ��
                //IntPtr dc2 = g2.GetHdc(); //�ѵ�ǰ��Ļ����λͼ������
                //BitBlt(dc2, 0, 0, this.Width, this.Height, dc3, this.Location.X, this.Location.Y, 13369376); //�ѵ�ǰ��Ļ������λͼ��
                //g1.ReleaseHdc(dc3); //�ͷ���Ļ���
                //g2.ReleaseHdc(dc2); //�ͷ�λͼ���
                
                // ��ȡPictrueBox����
                Graphics g1 = this.picMatch.CreateGraphics();
                // ����һ����ͼƬ
                Bitmap MyImage = new Bitmap(this.picMatch.Width, this.picMatch.Height);
                // ��ȡ��ͼƬ�Ļ���
                Graphics g2 = Graphics.FromImage(MyImage);
                // ��ȡ���ǵľ��
                IntPtr dc1 = g1.GetHdc();
                IntPtr dc2 = g2.GetHdc();
                // �������
                BitBlt(dc2, 0, 0, picMatch.Width, picMatch.Height, dc1, 0, 0, 13369376);
                // �ͷž��
                g1.ReleaseHdc(dc1);
                g2.ReleaseHdc(dc2);

                string ThePath = Application.StartupPath + "\\Image\\"; // ImageĿ¼ 
                string FileName = DateTime.Now.Date.ToString("yyyyMMdd") + DateTime.Now.ToString("HHmmss") + ".JPG";
                if (!System.IO.Directory.Exists(ThePath))
                {
                    // �����ͼĿ¼���������½�
                    System.IO.Directory.CreateDirectory(ThePath);
                }
                MyImage.Save(ThePath + "\\" + FileName, System.Drawing.Imaging.ImageFormat.Jpeg);

                ////�û��Լ�����ѡ��ͼƬ���ͣ�����λ�ã���������
                //SaveFileDialog saveFileDialog = new SaveFileDialog();
                //saveFileDialog.RestoreDirectory = true;
                //saveFileDialog.FileName = DateTime.Now.Date.ToString("yyyyMMdd") + DateTime.Now.ToString("HHmmss") + ".JPG";
                //if (saveFileDialog.ShowDialog() == DialogResult.OK)
                //{
                //    //����ļ�·��
                //    string localFilePath = saveFileDialog.FileName.ToString();
                //    //��ȡ�ļ���������·��
                //    string filename = localFilePath.Substring(0,localFilePath.LastIndexOf("\\"));
                    
                //    MyImage.Save(localFilePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                //}
            }
            catch
            {
            }
        }
        #endregion

        #region ��Ļ¼���� LiYoubing 20110806
        /// <summary>
        /// WM_COPYDATA���͵���Ϣ�����ݵ���������
        /// </summary>
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPStr)]
            public string lpData;
            //public IntPtr lpData;
        }

        /// <summary>
        /// API: Sends the specified message to a window or windows
        /// </summary>
        /// <param name="hWnd">A handle to the window whose window procedure will receive the message</param>
        /// <param name="Msg">The message to be sent</param>
        /// <param name="wParam">first message parameter: handle to destination window</param>
        /// <param name="lParam">second message parameter: user defined</param>
        /// <returns>Specifies the result of the message processing which depends on the message sent</returns>
        [System.Runtime.InteropServices.DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(int hWnd, int Msg, int wParam, ref COPYDATASTRUCT lParam);

        /// <summary>
        /// API: Sends the specified message to a window or windows
        /// </summary>
        /// <param name="hWnd">A handle to the window whose window procedure will receive the message</param>
        /// <param name="Msg">The message to be sent</param>
        /// <param name="wParam">first message parameter: handle to destination window</param>
        /// <param name="lParam">second message parameter: user defined</param>
        /// <returns>Specifies the result of the message processing which depends on the message sent</returns>
        [System.Runtime.InteropServices.DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(int hWnd, int Msg, int wParam, string lParam);

        /// <summary>
        /// API: Retrieves a handle to the top-level window whose class name and window name 
        /// match the specified strings
        /// </summary>
        /// <param name="lpClassName">The class name or a class atom</param>
        /// <param name="lpWindowName">The window name (the window's title)</param>
        /// <returns>A handle to the window that has the specified class name and window name or NULL</returns>
        [System.Runtime.InteropServices.DllImport("User32.dll", EntryPoint = "FindWindow")]
        private static extern int FindWindow(string lpClassName, string lpWindowName);

        /// <summary>
        /// API: Retrieves a handle to a window whose class name and window name match the specified strings
        /// </summary>
        /// <param name="hWndParent">A handle to the parent window whose child windows are to be searched</param>
        /// <param name="hWndChildAfter">A handle to a child window. The search begins with the next 
        /// child window in the Z order. The child window must be a direct child window of hwndParent, 
        /// not just a descendant window</param>
        /// <param name="lpClass">The class name or a class atom</param>
        /// <param name="lpWindow">The window name (the window's title)</param>
        /// <returns>A handle to the window that has the specified class name and window name or NULL</returns>
        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        private static extern int FindWindowEx(int hWndParent, int hWndChildAfter, string lpClass, string lpWindow);

        /// <summary>
        /// ��Ļ¼����Video/End��ť��Ӧ
        /// </summary>
        private void btnRecordVideo_Click(object sender, EventArgs e)
        {
            // ���µ���Video��ť��־��Ϊtrue��End��ť��Ϊfalse
            bool flag = true;
            if (btnRecordVideo.Text.Equals("Video(V)"))
            {
                btnRecordVideo.Text = "End(V)";
            }
            else
            {
                btnRecordVideo.Text = "Video(V)";
                flag = false;
            }

            // ��Ļ¼�����URWPGSim2D.Screencast�����ھ��
            int hWnd = 0;

            #region ����URWPGSim2D.Screencast������������
            // ������Ļ¼�����URWPGSim2D.Screencast�����ڻ�ȡ���ھ��
            hWnd = FindWindow(null, "URWPGSim2D.Screencast");
            if (hWnd == 0)
            {// Screencast�����ھ��û�л�ȡ��˵��Screencast��δ����
                if (flag == false)
                {// ���µ���End��ť����ʾ��ǰ����¼��
                    MessageBox.Show("URWPGSim2D.Screencast is not running", 
                        "Confirm", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.FileName = Application.StartupPath + "\\URWPGSim2D.Screencast.exe";
                try
                {
                    process.Start();
                    // ��ͣ1��ʹ��URWPGSim2D.Screencast������׼���ý���WM_COPYDATA��Ϣ
                    System.Threading.Thread.Sleep(1000);
                }
                catch
                {
                    //MessageBox.Show(process.StartInfo.FileName + "\ncan not be booted.\nPlease check it.", 
                    //    "Confirm", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                hWnd = FindWindow(null, "URWPGSim2D.Screencast");
                if (hWnd == 0)
                {
                    MessageBox.Show(process.StartInfo.FileName + "\ncan not be booted.\nPlease check it.", 
                        "Confirm", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            #endregion

            #region ������Ļ¼��Ĭ���ļ���
            // ��Ļ¼��Ĭ�ϱ���Ŀ¼
            string strFilePath = Application.StartupPath + "\\Video";
            if (!System.IO.Directory.Exists(strFilePath))
            {
                // ��Ļ¼��Ĭ�ϱ���Ŀ¼���������½�
                System.IO.Directory.CreateDirectory(strFilePath);
            }
            string strFileName = "";
            MyMission myMission = MyMission.Instance();
            for (int i = 0; i < myMission.ParasRef.TeamCount; i++)
            {// ʹ�ö�������ƴ����Ļ¼���ļ���
                strFileName += myMission.TeamsRef[i].Para.Name;
                if (i < myMission.ParasRef.TeamCount - 1)
                {
                    strFileName += ".";
                }
            }
            // ����������Ϊ����ʹ��Ĭ������test
            strFileName = string.IsNullOrEmpty(strFileName) ? "test." : strFileName + ".";
            strFileName = myMission.ParasRef.Name + "." + strFileName;
            DateTime now = DateTime.Now;
            strFileName += string.Format("{0:0000}{1:00}{2:00}{3:00}{4:00}{5:00}.wmv",
                now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
            #endregion

            #region ����ͨ��WM_COPYDATA��Ϣ���͵�string����
            COPYDATASTRUCT cds = new COPYDATASTRUCT();
            if (flag == true)
            {// ���µ���Video��ť��־��Ϊ1
                cds.dwData = (IntPtr)1;
                // ��Ļ¼����������ͳߴ�ȡpicMatch��������Ļ�ϵľ�������ͳߴ�
                // ��������ֵ�������ߴ�ֵ��4����������ʽ����5λʮ���������ַ���
                // ��ǰURWPGSim2D���������ھ����ʽ����10λʮ���������ַ���
                // ����Screencast���ڻ�ȡURWPGSim2D���������ڵ���ʾ����Ļ
                Point startPoint = picMatch.PointToScreen(new Point(0, 0));
                cds.lpData = "";
                // ��ʼλ�������ڶ���Ļ�����¿���Ϊ��
                // ��ʽ����1������λ�����Ż���ţ���5������λ
                //if (startPoint.X >= 0)
                //{ cds.lpData += string.Format("{0:+00000}", startPoint.X); }
                //else
                //{ cds.lpData += string.Format("{0:00000}", startPoint.X); }
                //if (startPoint.Y >= 0)
                //{ cds.lpData += string.Format("{0:+00000}", startPoint.Y); }
                //else
                //{ cds.lpData += string.Format("{0:00000}", startPoint.Y); }
                cds.lpData += string.Format("{0:+00000;-00000}{1:+00000;-00000}{2:000000}{3:000000}{4:0000000000}{5}", 
                    startPoint.X, startPoint.Y, picMatch.Width, picMatch.Height, (int)(this.Handle), 
                    strFilePath + "\\" + strFileName);
                cds.cbData = System.Text.Encoding.Default.GetBytes(cds.lpData).Length + 1;
            }
            else
            {// ���µ���End��ť��־��Ϊ0
                cds.dwData = IntPtr.Zero;
            }
            #endregion

            //const int WM_GETTEXT = 0x000D;
            //const int WM_CLICK = 0x00F5;
            //const int WM_SETTEXT = 0x000C;
            //int hWndEdit = 0;
            //hWndEdit = FindWindowEx(hWnd, 0, "TextBox", null);
            //if (hWndEdit != 0)
            //{
            //    SendMessage(hWndEdit, WM_SETTEXT, 0, startPoint.X.ToString());
            //}

            // WM_COPYDATA���͵���Ϣ��ϵͳ��Ĵ���Ϊ0x004A
            const int WM_COPYDATA = 0x004A;
            SendMessage(hWnd, WM_COPYDATA, 0, ref cds);
            if (flag == false)
            {// ���µ���End��ť��ȴ�URWPGSim2D.Screencast�˳�����ʾ
                // ��ͣ1��ʹ��URWPGSim2D.Screencast����WM_COPYDATA��Ϣ��ر����
                System.Threading.Thread.Sleep(1000);
                MessageBox.Show("Screencasting ended\nURWPGSim2D.Screencast exited successfully", 
                    "Confirm", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region ���������ͷ���ˮ��ȶ���Ķ�̬��Ϣ��ʾ���� Liushu
        // added by Liushu 20101128
        /// <summary>
        /// ��̬��Ϣ��ʾ���湦�ܣ�ʵʱ��ʾ�ͱ�����泡���ϸ���̬���󣨷��������ͷ���ˮ���˶�ѧ����
        /// </summary>
        private void btnDisplayFishInfo_Click(object sender, EventArgs e)
        {
            if (dlgFishInfo == null || dlgFishInfo.IsDisposed)
            {
                dlgFishInfo = new DlgFishInfo();
                dlgFishInfo.SetFishInfo();
            }

            dlgFishInfo.Show();
            dlgFishInfo.Focus();
        }

        // Liushu 20101128
        /// <summary>
        /// ����ǰ�����ŵ�ʹ���ж�̬���󣨷��������ͷ���ˮ�򣩲���������ʾ����
        /// </summary>
        /// <param name="displayFishInfo"></param>
        public void ProcessFishInfoDisplaying()
        {
            if ((dlgFishInfo != null) && (dlgFishInfo.IsDisposed == false))
            {
                MyMission.Instance().ParasRef.DisplayingCycles++;
                if ((MyMission.Instance().ParasRef.DisplayingCycles % dlgFishInfo.step) == 0)
                {
                    //���DisplayingCycles���������û�������ѡ��ĸ��²���step���ʱ������Ϊ0��20101129
                    MyMission.Instance().ParasRef.DisplayingCycles = 0;
                    dlgFishInfo.SetFishInfo();
                    if (dlgFishInfo.writeExcelDemo != null)
                    {
                        dlgFishInfo.writeExcelDemo.FishInfoWriteToExcel();
                    }
                }
            }
        }
        #endregion

        #region ���������ͷ���ˮ��ȶ���Ĺ켣���ƹ��� Weiqingdan
        // Weiqingdan 20101127
        /// <summary>
        /// ���ƹ켣���ܣ����Ƶ�ǰʹ�����������ͷ���ˮ��Ĺ켣
        /// </summary>
        private void btnDrawTrajectory_Click(object sender, EventArgs e)
        {
            if (dlgTrajectory == null || dlgTrajectory.IsDisposed)
            {
                dlgTrajectory = new DlgTrajectory();
            }
            dlgTrajectory.Show();
            dlgTrajectory.Focus();
        }

        /// <summary>
        /// ����ǰ�����ŵķ���ʹ���ж�̬���󣨷��������ͷ���ˮ�򣩹켣��������
        /// </summary>
        public void ProcessTrajectoryDrawing()
        {
            if ((dlgTrajectory != null) && (dlgTrajectory.IsDisposed == false))
            {
                MyMission myMission = MyMission.Instance();
                for (int i = 0; i < myMission.TeamsRef.Count; i++)
                {
                    for (int j = 0; j < myMission.TeamsRef[i].Fishes.Count; j++)
                    {
                        RoboFish f = MyMission.Instance().TeamsRef[i].Fishes[j];
                        myMission.TeamsRef[i].Fishes[j].TrajectoryPoints.Add(
                            Field.Instance().MmToPix(new Point((int)f.PositionMm.X, (int)f.PositionMm.Z)));
                    }
                }
                for (int i = 0; i < myMission.EnvRef.Balls.Count; i++)
                {// LiYoubing 20110104 ����null reference bug
                    myMission.EnvRef.Balls[i].TrajectoryPoints.Add(
                        Field.Instance().MmToPix(new Point((int)myMission.EnvRef.Balls[i].PositionMm.X,
                            (int)myMission.EnvRef.Balls[i].PositionMm.Z)));
                }
                dlgTrajectory.DrawPictureBox();
            }
        }
        #endregion

        private void ServerControlBoard_Load(object sender, EventArgs e)
        {
            // ��̬���ɴ����picMatch�ĳ��������˳�򲻿ɸı䣨���������������йأ� weiqingdan20101105
            Field f = Field.Instance();
            this.Width = f.FormLengthXPix + 10;     // 10Ϊ��������߿��������
            this.Height = f.FormLengthZPix + 32;    // 42Ϊ�������±߿��������
            int iSubtractFormLengthZPixAndPictureBoxZPix = f.FormLengthZPix - f.PictureBoxZPix;
            picMatch.Location = new Point(f.FormPaddingPix, iSubtractFormLengthZPixAndPictureBoxZPix / 2);
            picMatch.Width = f.PictureBoxXPix;
            picMatch.Height = f.PictureBoxZPix;

            // ˮ�������ʼ��
            InitWaveEffect();
            // ����ˮ����� LiYoubing 20110726
            chkWaveEffect.Checked = false; // Ĭ�Ϲر� 20110823
            // ͨ����ѡ��ť��ѡ�к�ȡ��״̬����ˮ����������úͽ���
            wave_timer.Enabled = chkWaveEffect.Checked;
            fish_timer.Enabled = chkWaveEffect.Checked;

            Bitmap bmp = URWPGSim2D.Gadget.WaveEffect.Instance().GetBitmap();
            // ������������Ϊ��ͼ�ؼ�PictureBox�ı���ͼƬ
            MissionCommonPara para = MyMission.Instance().ParasRef;
            if (para == null)
            {
                picMatch.BackgroundImage = Field.Instance().Draw(bmp, false, false);
            }
            else
            {
                picMatch.BackgroundImage = Field.Instance().Draw(bmp, para.IsGoalBlockNeeded, para.IsFieldInnerLinesNeeded);
            }
            //picMatch.BackgroundImage = f.Draw(bmp, MyMission.Instance().ParasRef.IsGoalBlockNeeded, MyMission.Instance().ParasRef.IsFieldInnerLinesNeeded);

            cmbCompetitionTime.SelectedIndex = 0;
            cmbCompetitionItem.SelectedIndex = 0;
        }

        #region Fieldѡ���Field Setting����泡�س���������Զ�����Ͽ�ؼ�������¼���Ӧ LiYoubing 20110627
        // LiYoubing 20110627
        void cmbFieldLength_Validated(object sender, EventArgs e)
        {
            int length = (int)Convert.ToSingle(cmbFieldLength.Text);
            int min = (int)Convert.ToSingle(cmbFieldLength.Items[0].ToString());
            if (length < min)
            {// ����ĳ��س���С�ڹ涨����С���ȣ��б�ĵ�0��ѡ��ֵ��
                MessageBox.Show(string.Format("Given length {0} mm is less than minimum length {1} mm.\n Canceled.", length, min), 
                    "URWPGSim2DServer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbFieldLength.SelectedIndex = 0;
                return;
            }

            cmbFieldLength_SelectedIndexChanged(null, null);
        }

        // LiYoubing 20110627
        private void cmbFieldLength_SelectedIndexChanged(object sender, EventArgs e)
        {
            Field f = Field.Instance();

            int length = (int)Convert.ToSingle(cmbFieldLength.Text);
            // �������õĳ��س���û�б仯�򲻱�ִ�к���һϵ���볡�سߴ��йصĸ��²��� LiYoubing 20110726
            if (length == f.FieldLengthXMm) { return; }

            cmbFieldWidth.Text = (length * f.FieldLengthZOriMm / f.FieldLengthXOriMm).ToString();
            f.FieldLengthXMm = (int)Convert.ToSingle(cmbFieldLength.Text);
            f.FieldLengthZMm = (int)Convert.ToSingle(cmbFieldWidth.Text);
            
            // �����趨�ĳ��س��ȺͿ�����¼���������Ҫ����ĳ��ز���
            f.FieldCalculation();
            MyMission.Instance().IMissionRef.ResetTeamsAndFishes();
            MyMission.Instance().IMissionRef.ResetBalls();
            MyMission.Instance().IMissionRef.ResetObstacles();

            Bitmap bmp = URWPGSim2D.Gadget.WaveEffect.Instance().GetBitmap();
            // �ػ���泡��
            SetFieldBmp(f.Draw(bmp, MyMission.Instance().ParasRef.IsGoalBlockNeeded, MyMission.Instance().ParasRef.IsFieldInnerLinesNeeded));

            // �ػ����ʹ������
            SetMatchBmp(MyMission.Instance().IMissionRef.Draw());

            //// ģ���л�һ��TabControl��ѡ�����ı䳡�ش�С���һ��˫�����ز���ȫ������
            //// ����������������
            //tabServerControlBoard.SelectedIndex = -1;
            //tabServerControlBoard.SelectedIndex = 2;
        }
        #endregion

        //added by Liushu 20101121
        /// <summary>
        /// Server�յ�Ready��Ϣ�󣬴����������ʾ���û����档
        /// </summary>
        /// <param name="teamId">��ǰ��Ϣ��Դ�ͻ��˴���Ķ����ڷ����TeamsRef�б��еı��</param>
        public void SetTeamReadyState(int teamId)
        {
            _listTeamStrategyControls[teamId].BtnReady.Enabled = false;

            if (btnPause.Enabled == false)  //modified by liushu 20110308
            {
                bool flag = false;
                for (int i = 0; i < _listTeamStrategyControls.Count; i++)
                {// ֻҪ����Ready��ť��δ������flagΪtrue
                    flag = flag || _listTeamStrategyControls[i].BtnReady.Enabled;
                }
                if (flag == false)
                {// ����Ready��ť���Ѱ��������Start
                    btnStart.Enabled = true;
                }
            }
            else 
            {
                btnStart.Enabled = false;
            }

            btnRestart.Enabled = true;
        }

        /// <summary>
        /// ������ʹ�����ж�̬�仯��˽�б���ֵ�Ϳؼ�״̬
        /// </summary>
        public void ResetPrivateVarsAndControls()
        {
            rdoRemote.Focus();  // �ý���ͣ����rdoRemote�ؼ���

            StateRestart();

            // ���ûط���ر���
            _curCachingIndex = 0;
            _curReplayingIndex = 0;
            _isReplaying = false;
            
            // ���ÿ��ܴ��ڲ�ͬ״̬�İ�ť
            btnPause.Text = "Pause(P)";
            btnReplay.Text = "Replay(Y)";

            // ��������ģʽΪRemoteģʽ
            MyMission.Instance().IsRomteMode = true;
            rdoRemote.Checked = true;
            _isThereTeamReady = false;

            if ((dlgFishInfo != null) && (dlgFishInfo.IsDisposed == false))
            {// �����̬�����˶�ѧ����ʵʱ��ʾ�����Ǵ򿪵���������
                dlgFishInfo.Dispose();
                btnDisplayFishInfo_Click(btnDisplayFishInfo, new EventArgs());
            }

            if ((dlgTrajectory != null) && (dlgTrajectory.IsDisposed == false))
            {// �����̬����켣���ƴ����Ǵ򿪵���������
                dlgTrajectory.Dispose();
                btnDrawTrajectory_Click(btnDrawTrajectory, new EventArgs());
            }
        }

        /// <summary>
        /// ��Server������CLOSED��Ϣ��ʹ��Stop Service��Shutdown Dss Node
        /// </summary>
        private void ServerControlBoard_FormClosed(object sender, FormClosedEventArgs e)
        {
            //FromServerUiEvents.Instance().Post(new FromServerUiMsg(FromServerUiMsg.MsgEnum.CLOSED, new string[] { }));
            // �������ʹ��Excel���汨���������ȹر�֮
            if ((dlgFishInfo != null) && (dlgFishInfo.writeExcelDemo != null))
            {
                dlgFishInfo.writeExcelDemo.CloseExcel();
            }
            Process.GetCurrentProcess().Kill(); // û��������Ҫ����ֱ�ӽ�����ǰ����
        }

        /// <summary>
        /// ���û�����ʾ�ĶԻ�����ʾ��Ϣ  20101215
        /// </summary>
        /// <param name="dialogInfo"></param>
        public void ShowDialogInfo(string dialogInfo)
        {
            MessageBox.Show(string.Format("{0}", dialogInfo), "Confirm", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        #region ˮ����� Solesschong 20110712
         /*=======================================ˮ�����ԭ��============================================*
        / * FastBitmap��ˮ������ڲ������ݽṹ���������ò��ꡣ���� 
          * ˮ��Ч��Դ���򱻷�װ��WaveEffect����
          *      ����ӿ�Ϊ LoadBmp, DropWater, PaintWater, GetBitmap. 
          *      LoadBmp�ǳ�ʼ��������
          *      GetBitmap��������������� 
          * �����������
          *      ������һ��ͼƬ��ʼ����֮��������λ����DropWater��������Ϊˮ�����ˮ��Դ��
          *      ����һ��Ƶ�ʵ���PaintWater��ˢ��ˮ��ͼƬ��
          *      ����GetBitmap���ص�ͼƬ����Field.Draw()����
          *      ����ֵ����picMatch.BackgroundImage��
          * /=============================================================================================*
          */

        /// <summary>
        /// ˮ��Ч������ LiYoubing 20110726
        /// </summary>
        private void chkWaveEffect_CheckedChanged(object sender, EventArgs e)
        {
            // ͨ����ѡ��ť��ѡ�к�ȡ��״̬����ˮ����������úͽ���
            wave_timer.Enabled = chkWaveEffect.Checked;
            fish_timer.Enabled = chkWaveEffect.Checked;

            // ˮ�������ʼ��
            InitWaveEffect();
            // �ػ汳��
            Bitmap bmp = URWPGSim2D.Gadget.WaveEffect.Instance().GetBitmap();
            MissionCommonPara para = MyMission.Instance().ParasRef;
            if (para == null)
            {
                picMatch.BackgroundImage = Field.Instance().Draw(bmp, false, false);
            }
            else
            {
                picMatch.BackgroundImage = Field.Instance().Draw(bmp, para.IsGoalBlockNeeded, para.IsFieldInnerLinesNeeded);
            }
            picMatch.Invalidate();
        }

        /// <summary>
        /// ˮ�������ʼ��
        /// </summary>
        private void InitWaveEffect()
        {
            if (picLoader.Image != null) { picLoader.Image.Dispose(); }

            picLoader.Image = new Bitmap(Application.StartupPath + "\\URWPGSim2D.FieldBackground.bmp");

            WaveEffect.Instance().LoadBmp((Bitmap)picLoader.Image);
        }
        /// <summary>
        /// ��װ�Ĳ���ˮ������
        /// bool��ʾ�Ƿ�Ϊ����ģʽ
        /// </summary>
        /// <param name="IsPossibility"></param>
        private void DropAllFish(bool IsPossibility)
        {
            int p = 0;
            double size = 4.0 / Field.Instance().ScaleMmToPix;
            foreach (Ball ball in MyMission.Instance().EnvRef.Balls)
            {
                int x, z;
                x = (int)ball.PositionMm.X;
                z = (int)ball.PositionMm.Z;
                x = Field.Instance().MmToPixX((int)x);
                z = Field.Instance().MmToPixZ((int)z);
                WaveEffect w = WaveEffect.Instance();
                // ����ˮ���ĸ���
                double v = Math.Sqrt((ball.PositionMm.X - ball.PrePositionMm.X) * (ball.PositionMm.X - ball.PrePositionMm.X) 
                    + (ball.PositionMm.Z - ball.PrePositionMm.Z) * (ball.PositionMm.Z - ball.PrePositionMm.Z));
                if (IsPossibility)
                {
                    if (v > 9 && v < 320)
                    {
                        p = (int)(WaveEffect.Instance().GetRandom((int)v * 400));
                    }
                    else
                    {
                        p = w.GetRandom(1000);
                    }
                }
                else  // (!IsPossibility) �Ǹ���ģʽ
                {
                    p = 1000;
                }
                if (p > 900)
                {
                    w.DropWater(x, z, (int)(size * (25 + w.GetRandom(5))), 10 + w.GetRandom(50));
                }
            }

            foreach (Team<RoboFish> team in MyMission.Instance().TeamsRef)
            {
                foreach (RoboFish fish in team.Fishes)
                {
                    // �����λ��
                    int x, z, len; float dir;
                    x = (int)fish.PositionMm.X;
                    z = (int)fish.PositionMm.Z;
                    len = (int)(fish.BodyLength / Field.Instance().ScaleMmToPix);
                    dir = fish.BodyDirectionRad;
                    x = Field.Instance().MmToPixX((int)x);
                    z = Field.Instance().MmToPixZ((int)z);
                    WaveEffect w = WaveEffect.Instance();

                    // ����ˮ���ĸ���
                    double v = Math.Sqrt((fish.PositionMm.X - fish.PrePositionMm.X) * (fish.PositionMm.X - fish.PrePositionMm.X) 
                        + (fish.PositionMm.Z - fish.PrePositionMm.Z) * (fish.PositionMm.Z - fish.PrePositionMm.Z));
                    int unitlen = 10;
                    if (IsPossibility)
                    {
                        if (v > 9 && v < 320)
                        {
                            p = (int)(WaveEffect.Instance().GetRandom((int)v * 400));
                        }
                        else
                        {
                            p = w.GetRandom(1000);
                        }
                    }
                    else  // (!IsPossibility) �Ǹ���ģʽ
                    {
                        p = 1000;
                    }
                    if (p > 900)
                    {
                        for (int i = -len / 2; i < len / 2; i += unitlen)
                        {
                            if (WaveEffect.Instance().GetRandom(2) < 1)
                            {
                                w.DropWater(x - (int)(Math.Cos(dir) * i), z - (int)(Math.Sin(dir) * i),
                                    (int)(size * (10 + w.GetRandom(5))), 10 + w.GetRandom(400));
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ��ʱˢ��ˮ��ͼƬ,����ˮ����ɢ 
        /// �ٶ��д����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wave_timer_Tick(object sender, EventArgs e)
        {
            // ��֮ͣ��һ��������λ��ͻ�䣬��Ҫ��ʼ�����
            if (MyMission.Instance().ParasRef.IsPaused)
                InitWaveEffect();

            WaveEffect.Instance().PaintWater();
            Bitmap bmp = URWPGSim2D.Gadget.WaveEffect.Instance().GetBitmap();
            picMatch.BackgroundImage = Field.Instance().Draw(bmp, MyMission.Instance().ParasRef.IsGoalBlockNeeded, MyMission.Instance().ParasRef.IsFieldInnerLinesNeeded);
            picMatch.Invalidate();
        }

        /// <summary>
        /// ��ʱ����ˮ��Դ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fish_timer_Tick(object sender, EventArgs e)
        {
            DropAllFish(true);
            // �ò���ˮ����ʱ�䲻ȷ����Ч������ʵ
            fish_timer.Interval = 500 + WaveEffect.Instance().GetRandom(300);
        }
        #endregion

        #region �������ֹ��� LiYoubing 20110711
        /// <summary>
        /// ���ű��������õĲ���������
        /// </summary>
        AudioPlayer _player = new AudioPlayer();

        /// <summary>
        /// ��ǰ�Ѿ�ѡ��ı������������ļ���(��·��)
        /// </summary>
        string _strMusicFullName = "";

        /// <summary>
        /// �������������ť����¼���Ӧ �����ļ�ѡ��Ի��� ѡ�񱳾������ļ�
        /// </summary>
        private void btnMusicBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "audio files (*.mp3;*.wav)|*.mp3;*.wav|All files (*.*)|*.*";  // �����ļ�����
            openFileDialog.ShowReadOnly = true; // �趨�ļ��Ƿ�ֻ��
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtMusicName.Text = openFileDialog.SafeFileName;    // ��ȡ�ļ���
                _strMusicFullName = openFileDialog.FileName;        // ��ȡ��������·�����ļ���
            }
        }

        /// <summary>
        /// �������ֲ��Ű�ť����¼���Ӧ
        /// ��ʼѭ�����ű�������
        /// </summary>
        private void btnMusicPlay_Click(object sender, EventArgs e)
        {
            _player.FileName = _strMusicFullName;
            _player.Play();
            btnMusicPause.Enabled = true;
            btnMusicStop.Enabled = true;
        }

        /// <summary>
        /// ����������ͣ/������ť����¼���Ӧ
        /// ��ͣ��������ű�������
        /// </summary>
        private void btnMusicPause_Click(object sender, EventArgs e)
        {
            if (btnMusicPause.Text.Equals("Pause"))
            {
                btnMusicPause.Text = "Resume";
                _player.Pause();
            }
            else
            {
                btnMusicPause.Text = "Pause";
                _player.Resume();
            }
        }

        /// <summary>
        /// ��������ֹͣ��ť����¼���Ӧ
        /// ֹͣ���ű�������
        /// </summary>
        private void btnMusicStop_Click(object sender, EventArgs e)
        {
            _player.Stop();
            btnMusicPause.Enabled = false;
            btnMusicStop.Enabled = false;
            btnMusicPause.Text = "Pause";
        }

        /// <summary>
        /// �����ͣ�ڱ�������������ʾ����ʱ��ʾ�������������ļ�������·����
        /// </summary>
        private void txtMusicName_MouseHover(object sender, EventArgs e)
        {
            // ����껬��txtMusicNamʱ�����洢�ı��������ļ�����·����ʾ����
            ToolTip tip = new ToolTip();
            tip.SetToolTip(txtMusicName, _strMusicFullName);
        }

        #endregion
    }
}