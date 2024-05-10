using AnotherECS.Core.Allocators;
using System;
using System.Collections.Generic;

namespace AnotherECS.Core.Caller
{
    internal static class CallerDeclaration
    { 
        public static (GenericDeclaration TAllocator, GenericDeclaration TSparse, GenericDeclaration TDenseIndex) GetLayoutDeclaration(in TypeOptions option)
        {
            Type TSparse = null;
            Type TDenseIndex = null;

            switch (option.sparseMode)
            {
                case TypeOptions.SparseMode.Bool:
                    {
                        TSparse = typeof(bool);
                        if (option.isSingle || (option.isMarker && option.isEmpty))
                        {
                            TDenseIndex = typeof(uint);
                        }
                        else
                        {
                            TDenseIndex =  typeof(ushort);
                        }
                        break;
                    }
                case TypeOptions.SparseMode.Ushort:
                    {
                        TSparse = typeof(ushort);
                        TDenseIndex = typeof(ushort);
                        break;
                    }
            }

            Type TAllocator;
            if (option.isHistory)
            {
                TAllocator = typeof(HAllocator);
            }
            else
            {
                TAllocator = typeof(BAllocator);
            }

            return (
                new GenericDeclaration(TAllocator),
                new GenericDeclaration(TSparse),
                new GenericDeclaration(TDenseIndex)
                );
        }

        public static GenericDeclaration GetCallerDeclaration(in TypeOptions option)
        {
            var (TAllocator, TSparse, TDenseIndex) = GetLayoutDeclaration(option);
            var TComponent = new GenericDeclaration(option.type);

            List<GenericDeclaration> layoutASCD = new()
            {
                TAllocator, TSparse, TComponent, TDenseIndex
            };

            List<GenericDeclaration> layoutASC = new()
            {
                TAllocator, TSparse, TComponent
            };

            List<GenericDeclaration> layoutAC = new()
            {
                TAllocator, TComponent
            };

            List<GenericDeclaration> layoutC = new()
            {
                TComponent
            };

            List<GenericDeclaration> layoutS = new()
            {
                TSparse
            };

            var nothingSCDTC = new GenericDeclaration(typeof(Nothing<,,,>), layoutASCD);

            var singleFeature = new GenericDeclaration(typeof(SingleCF<,,,>), layoutASCD);

            var caller = new GenericDeclaration(typeof(Caller<,,,,,,,,,,,,,,,,,,>));

            caller.Generic.AddRange(layoutASCD);

#if ANOTHERECS_HISTORY_DISABLE
            if (false)
#else
            if (option.isHistory)
#endif
            {
                caller.Generic.Add(new GenericDeclaration(typeof(HistoryAllocatorCF)));
            }
            else
            {
                caller.Generic.Add(new GenericDeclaration(typeof(NoHistoryAllocatorCF)));
            }

            if (option.isSingle || (option.isMarker && option.isEmpty))
            {
                caller.Generic.Add(new GenericDeclaration(typeof(UintNumber)));
            }
            else
            {
                caller.Generic.Add(new GenericDeclaration(typeof(UshortNumber)));
            }

            
            if (option.isInject)
            {
                caller.Generic.Add(new GenericDeclaration(typeof(InjectCF<,,,>), layoutASCD));
            }
            else
            {
                caller.Generic.Add(nothingSCDTC);
            }
            
            if (option.isUseRecycle)
            {
                caller.Generic.Add(new GenericDeclaration(typeof(RecycleStorageCF<,,,>), layoutASCD));
            }
            else if (option.isSingle)
            {
                caller.Generic.Add(new GenericDeclaration(typeof(SingleStorageCF<,,,>), layoutASCD));
            }
            else
            {
                caller.Generic.Add(new GenericDeclaration(typeof(IncrementStorageCF<,,,>), layoutASCD));
            }
            
            if (option.isDefault)
            {
                caller.Generic.Add(new GenericDeclaration(typeof(DefaultCF<,>), layoutAC));
            }
            else
            {
                caller.Generic.Add(nothingSCDTC);
            }
            
            if (option.isAttachExternal || option.isDetachExternal)
            {
                caller.Generic.Add(new GenericDeclaration(typeof(AttachDetachExternalCF<,,,>), layoutASCD));
            }
            else
            {
                caller.Generic.Add(nothingSCDTC);
            }
            
            if (option.isAttachExternal)
            {
                caller.Generic.Add(new GenericDeclaration(typeof(AttachExternalCF<,,,>), layoutASCD));
            }
            else
            {
                caller.Generic.Add(nothingSCDTC);
            }
            
            if (option.isDetachExternal)
            {
                caller.Generic.Add(new GenericDeclaration(typeof(DetachExternalCF<,,,>), layoutASCD));
            }
            else
            {
                caller.Generic.Add(nothingSCDTC);
            }
            
            if (option.isSingle)
            {
                caller.Generic.Add(new GenericDeclaration(typeof(SingleSparseCF<,>), layoutAC));
            }
            else
            {
                if (option.sparseMode == TypeOptions.SparseMode.Bool || option.isEmpty)
                {
                    if (option.isMarker)
                    {
                        caller.Generic.Add(new GenericDeclaration(typeof(NonSparseCF<,>), layoutAC));
                    }
                    else
                    {
                        caller.Generic.Add(new GenericDeclaration(typeof(BoolSparseCF<,>), layoutAC));
                    }
                }
                else if (option.sparseMode == TypeOptions.SparseMode.Ushort)
                {
                    caller.Generic.Add(new GenericDeclaration(typeof(UshortSparseCF<,>), layoutAC));
                }
            }

            if (option.isEmpty)
            {
                caller.Generic.Add(new GenericDeclaration(typeof(EmptyCF<,,,>), layoutASCD));
            }
            else
            {
                if (option.isSingle)
                {
                    caller.Generic.Add(singleFeature);
                }
                else
                {
                    caller.Generic.Add(new GenericDeclaration(typeof(UshortDenseCF<,,>), layoutASC));
                }
            }
            
            if (option.isBindToEntity)
            {
                if (option.isMarker)
                {
                    caller.Generic.Add(new GenericDeclaration(typeof(TempBinderToFiltersCF)));
                }
                else
                {
                    caller.Generic.Add(new GenericDeclaration(typeof(BinderToFiltersCF)));
                }
            }
            else
            {
                caller.Generic.Add(nothingSCDTC);
            }
            
            if (option.isVersion)
            {
                if (option.isSingle)
                {
                    caller.Generic.Add(new GenericDeclaration(typeof(UintVersionCF<,,>), layoutASC));
                }
                else
                {
                    caller.Generic.Add(new GenericDeclaration(typeof(UshortVersionCF<,,>), layoutASC));
                }
            }
            else
            {
                caller.Generic.Add(nothingSCDTC);
            }

            caller.Generic.Add(new GenericDeclaration(typeof(BSerializeCF<,,,>), layoutASCD));

            if (option.isRepairMemory)
            {
                caller.Generic.Add(new GenericDeclaration(typeof(RepairMemoryCF<>), layoutC));
            }
            else
            {
                caller.Generic.Add(nothingSCDTC);
            }

            if (option.isRepairStateId)
            {
                caller.Generic.Add(new GenericDeclaration(typeof(RepairStateIdCF<>), layoutC));
            }
            else
            {
                caller.Generic.Add(nothingSCDTC);
            }
            return caller;
        }
    }
}
