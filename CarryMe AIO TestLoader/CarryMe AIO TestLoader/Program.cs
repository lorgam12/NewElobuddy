﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarryMe_Collection;
using EloBuddy.SDK.Events;

namespace CarryMe_AIO_TestLoader
{
	class Program
	{
		static void Main(string[] args)
		{
			Loading.OnLoadingComplete += Loading_OnLoadingComplete;
		}
		private static void Loading_OnLoadingComplete(EventArgs args)
		{
			new Champions();
		}
	}
}
