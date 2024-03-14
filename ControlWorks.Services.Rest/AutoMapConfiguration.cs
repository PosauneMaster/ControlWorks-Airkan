﻿using AutoMapper;

using ControlWorks.Services.Rest.Models;

namespace ControlWorks.Services.Rest
{
    public static class AutoMapConfiguration
    {
        public static IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<RecipeItem, BinItemActive>();
                cfg.CreateMap<Recipe, RecipeActiveDto>();
                cfg.CreateMap<RecipeItem, RecipeItemDto>();
                cfg.CreateMap<UnmatchedRecipeItemDto, UnmatchedRecipeItem>();
                cfg.CreateMap<RecipeActiveDto, RecipeComplete>();
                cfg.CreateMap<RecipeItemDto, RecipeCompleteItem>();
                cfg.CreateMap<RecipeCompleteItem, BinItemComplete>();
                cfg.CreateMap<Alarm, Alarm>();
                cfg.CreateMap<AlarmCollection, AlarmCollection>();

            });

            var mapper = new Mapper(config);

            return mapper;

        }
    }

}
