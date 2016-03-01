﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using EthansList.Shared;

namespace EthansList.MaterialDroid
{
    public class SelectCityView : LinearLayout
    {
        ListView city_picker, state_picker;
        Context context;
        CityListAdapter cityAdapter;
        StateListAdapter stateAdapter;
        protected string state;


        public SelectCityView(Context context) :
            base(context)
        {
            this.context = context;
            Initialize();
        }

        public SelectCityView(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            this.context = context;
            Initialize();
        }

        public SelectCityView(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            this.context = context;
            Initialize();
        }

        void Initialize()
        {
            this.WeightSum = 1;
            this.Orientation = Orientation.Horizontal;
            LayoutParams p = new LayoutParams(0, ViewGroup.LayoutParams.MatchParent, 0.5f);
            AvailableLocations locations = new AvailableLocations();
            state = locations.States.ElementAt(0);

            state_picker = new ListView(context);
            state_picker.LayoutParameters = p;
            state_picker.Adapter = new StateListAdapter(context, locations.States);
            AddView(state_picker);

            city_picker = new ListView(context);
            city_picker.LayoutParameters = p;
            city_picker.Adapter = new CityListAdapter(context, locations.PotentialLocations.Where(loc => loc.State == state));
            AddView(city_picker);

            state_picker.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {
                state = locations.States.ElementAt(e.Position);
                cityAdapter = new CityListAdapter(context, locations.PotentialLocations.Where(loc => loc.State == state));
                city_picker.Adapter = cityAdapter;
            };

            city_picker.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) =>
            {
                Location selected = locations.PotentialLocations.Where(loc => loc.State == state).ElementAt(e.Position);
                //FragmentTransaction transaction = this.context.FragmentManager.BeginTransaction();
                FragmentTransaction transaction = ((Activity)context).FragmentManager.BeginTransaction();
                //SearchFragment searchFragment = new SearchFragment();
                //searchFragment.location = selected;
                //SearchOptionsFragment searchFragment = new SearchOptionsFragment();
                CategoryPickerFragment categoryFragment = new CategoryPickerFragment();

                transaction.Replace(Resource.Id.frameLayout, categoryFragment);
                transaction.AddToBackStack(null);
                transaction.Commit();

                Task.Run(async () =>
                {
                    await MainActivity.databaseConnection.AddNewRecentCityAsync(selected.SiteName, selected.Url).ConfigureAwait(true);

                    if (MainActivity.databaseConnection.GetAllRecentCitiesAsync().Result.Count > 5)
                        await MainActivity.databaseConnection.DeleteOldestCityAsync().ConfigureAwait(true);
                });
            };
        }
    }
}
