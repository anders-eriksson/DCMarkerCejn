using Configuration;
using Contracts;
using DCLog;
using DCMarker.Flexible;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using GlblRes = global::DCMarker.Properties.Resources;
using System.Collections;
using System.Diagnostics;

namespace DCMarker
{
    public partial class FlexibleMainViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        private ICommand _OkButtonCommand;

        public ICommand OkButtonCommand
        {
            get
            {
                if (_OkButtonCommand == null)
                {
                    _OkButtonCommand = new RelayCommand(
                        p => this.CanOkButtonCommandExecute(),
                        p => this.DoOkButtonCommand());
                }
                return _OkButtonCommand;
            }
        }

        private bool CanOkButtonCommandExecute()
        {
            var result = IsValid;
            //bool result = false;
            //if (HasTOnr)
            //{
            //    result = !HasInputError; // !string.IsNullOrWhiteSpace(ArticleNumber) && !string.IsNullOrWhiteSpace(Quantity) && TOnr.Length == DCConfig.Instance.ToNumberLength;
            //}
            //else
            //{
            //    result = !string.IsNullOrWhiteSpace(ArticleNumber) && !string.IsNullOrWhiteSpace(Quantity);
            //}

            return result;
        }

        public bool ArticleFound { get; set; }

        private void DoOkButtonCommand()
        {
            int test;
            if (ArticleFound)
            {
                ErrorMessage = string.Empty;
                Article article = new Article()
                {
                    F1 = ArticleNumber,
                    //Kant = Kant,
                    FixtureId = Fixture,
                    EnableTO = HasTOnr,
                    TOnumber = TOnr,
                    Template = string.Empty,
                    IsTestItemSelected = IsTestItemSelected
                };
                BatchesDone = 0;
                //OrderInProgress = false;
                OrderNotStarted = false;
                ItemsDone = "";
                //TableSide = TableName[0];

                _wf.UpdateWorkflow(article);
                _wf.UpdateTOnumber(TOnr);

                // TODO should FirstMarkingResetZ be used ???
                //_wf.FirstMarkingResetZ = true;

                Status = GlblRes.Waiting_for_product;
            }
            //else
            //{
            //    ErrorMessage = GlblRes.article
            //}
            //ResetZAxis();
        }

        private ICommand _CancelButtonCommand;

        public ICommand CancelButtonCommand
        {
            get
            {
                if (_CancelButtonCommand == null)
                {
                    _CancelButtonCommand = new RelayCommand(
                        p => this.CanCancelButtonCommandExecute(),
                        p => this.DoCancelButtonCommand());
                }
                return _CancelButtonCommand;
            }
        }

        private bool CanCancelButtonCommandExecute()
        {
            return true;
        }

        private void DoCancelButtonCommand()
        {
            ErrorMessage = string.Empty;

            _wf.ResetCareful();
            _wf.ResetArticleReady();
            _wf.ResetNextToLast();
            _wf.ResetItemsDone();
            ResetInputValues();
            Status = string.Empty;

            // TODO remove this since we don't use it.... // AME 2017-06-13
            OrderInProgress = true;
            OrderNotStarted = true;
        }

        private ICommand _ArticleChanged;

        public ICommand ArticleChanged
        {
            get
            {
                return _ArticleChanged;
            }
            set
            {
                _ArticleChanged = value;
                NotifyPropertyChanged();
            }
        }

        private ICommand _LoadArticleCommand;

        public ICommand LoadArticleCommand
        {
            get
            {
                if (_LoadArticleCommand == null)
                {
                    _LoadArticleCommand = new RelayCommand(
                        p => this.CanLoadArticleCommandExecute(),
                        p => this.DoLoadArticleCommand());
                }
                return _LoadArticleCommand;
            }
        }

        private string DoLoadArticleCommand()
        {
            string result = null;

            ErrorMessage = string.Empty;
            Status = string.Empty;
            if (!string.IsNullOrWhiteSpace(ArticleNumber))
            {
                List<Article> dbResult = _wf.GetArticle(ArticleNumber);
                if (dbResult.Count > 0)
                {
                    Fixture = dbResult[0].FixtureId;
                    HasFixture = string.IsNullOrWhiteSpace(Fixture) ? false : true;

                    bool? enableTO = dbResult[0].EnableTO;
                    // only have to check on the first Edge/Kant
                    HasTOnr = enableTO.HasValue ? enableTO.Value : false;

                    bool? careful = dbResult[0].Careful;
                    IsCareful = careful.HasValue ? careful.Value : false;

                    TOnr = string.Empty;

                    if (dbResult.Count > 1)
                    {
                        // the article has edges/kant
                        HasKant = true;
                    }
                    else
                    {
                        HasKant = false;
                    }
                    ArticleFound = true;
                }
                else
                {
                    // the article doesn't exists show an error!!!
                    ErrorMessage = GlblRes.Article_does_not_exist_in_database;
                    result = ErrorMessage;

                    HasTOnr = false;
                    ArticleFound = false;
                }
            }
            return result;
        }

        private bool CanLoadArticleCommandExecute()
        {
            return true;
        }

        private ICommand _ExecuteMarkingCommand;

        public ICommand ExecuteMarkingCommand
        {
            get
            {
                if (_ExecuteMarkingCommand == null)
                {
                    _ExecuteMarkingCommand = new RelayCommand(
                        p => this.CanExecuteMarkingCommandExecute(),
                        p => this.DoExecuteMarkingCommand());
                }
                return _ExecuteMarkingCommand;
            }
        }

        private void DoExecuteMarkingCommand()
        {
            if (_wf != null)
                _wf.Execute();
        }

        private bool CanExecuteMarkingCommandExecute()
        {
            // change this to the actual condition that should result to activate the command
            return true;
        }
    }
}