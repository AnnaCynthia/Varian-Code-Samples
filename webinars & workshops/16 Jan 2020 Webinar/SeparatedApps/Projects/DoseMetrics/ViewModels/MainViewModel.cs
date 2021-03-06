﻿using DoseMetrics.Events;
using DoseMetrics.Models;
using Microsoft.Win32;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace DoseMetrics.ViewModels
{
    public class MainViewModel
    {
        public DelegateCommand SaveTemplateCommand { get; private set; }
        public DelegateCommand LoadMetricsCommand { get; private set; }
        public MainViewModel(DoseMetricSelectionViewModel doseMetricSelectionViewModel,
            DoseMetricViewModel doseMetricViewModel,
            IEventAggregator eventAggregator,
            PlanSetup plan)
        {
            _plan = plan;
            DoseMetricViewModel = doseMetricViewModel;
            DoseMetricSelectionViewModel = doseMetricSelectionViewModel;
            _eventAggregator = eventAggregator;
            SaveTemplateCommand = new DelegateCommand(OnSaveTemplate);
            LoadMetricsCommand = new DelegateCommand(OnLoadTemplate);
        }

        private void OnSaveTemplate()
        {
            if (DoseMetricViewModel.DoseMetrics.Count() > 0)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "JSON (*.json)|*.json";
                if (sfd.ShowDialog() == true)
                {
                    using (StreamWriter sw = new StreamWriter(sfd.FileName))
                    {
                        sw.Write(JsonConvert.SerializeObject(DoseMetricViewModel.DoseMetrics));
                    }
                }
            }
            
        }

        private void OnLoadTemplate()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "JSON (*.json)|*.json";
            if(ofd.ShowDialog() == true)
            {
                foreach(var dm in JsonConvert.DeserializeObject<List<DoseMetricModel>>(File.ReadAllText(ofd.FileName)))
                {
                    var dmm = new DoseMetricModel(_plan)
                    {
                        InputUnit = dm.InputUnit,
                        InputUnits = dm.InputUnits,
                        InputValue = dm.InputValue,
                        Metric = dm.Metric,
                        OutputUnit = dm.OutputUnit,
                        OutputUnits = dm.OutputUnits,
                        Structure = dm.Structure,
                        Tolerance = dm.Tolerance
                    };
                    dmm.GetOutputValue();
                    _eventAggregator.GetEvent<AddDoseMetricEvent>().Publish(dmm);
                }
            }
        }

        private PlanSetup _plan;

        public DoseMetricViewModel DoseMetricViewModel { get; }
        public DoseMetricSelectionViewModel DoseMetricSelectionViewModel { get; }

        private IEventAggregator _eventAggregator;
    }
}
