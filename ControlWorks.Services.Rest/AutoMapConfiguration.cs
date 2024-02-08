using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoMapper;
using ControlWorks.Services.PVI.Variables.Models;
using ControlWorks.Services.Rest.Data;
using ControlWorks.Services.Rest.Models;

namespace ControlWorks.Services.Rest
{
    public static class AutoMapConfiguration
    {
        public static IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<RecipeItem, BinItemActive>();
                cfg.CreateMap<BinItemActive, BinItem>();
                cfg.CreateMap<Recipe, RecipeActiveDto>();
                cfg.CreateMap<RecipeItem, RecipeItemDto>();
                cfg.CreateMap<RecipeItemDto, BinItem>();
                cfg.CreateMap<UnmatchedRecipeItemDto, UnmatchedRecipeItem>();
                cfg.CreateMap<RecipeActiveDto, RecipeComplete>();
                cfg.CreateMap<RecipeItemDto, RecipeCompleteItem>();
                cfg.CreateMap<RecipeCompleteItem, BinItemComplete>();
                cfg.CreateMap<Alarm, Alarm>();
                cfg.CreateMap<AlarmCollection, AlarmCollection>();
                cfg.CreateMap<VerizonOrderInfo, VerizonOrderInfo>()
                    .ForMember(v => v.Items,
                        opt => opt.Ignore());

                cfg.CreateMap<List<VerizonOrderInfo>, List<VerizonOrderInfo>>();
                cfg.CreateMap<SortItem, SortItem>();

            });

            var mapper = new Mapper(config);

            return mapper;

        }
    }

}
