using FreeSql.Internal.ObjectPool;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using System.Transactions;
using Wombat;
using Wombat.Core.DependencyInjection;
using IsolationLevel = System.Data.IsolationLevel;

namespace Wombat.Web.Infrastructure
{
    /// <summary>
    /// 使用事务包裹
    /// </summary>
    public class TransactionalAttribute : AOPBaseAttribute
    {
        private readonly IsolationLevel _isolationLevel;
        public TransactionalAttribute(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            _isolationLevel = isolationLevel;
        }
        private TransactionContainer _container;

        public override  void Before(IAOPContext context)
        {
            _container = context.ServiceProvider.GetService<TransactionContainer>();

            if (!_container.TransactionOpened)
            {
                _container.TransactionOpened = true;
                _container.BeginTransactionAsync(_isolationLevel);
            }
        }
        public override void After(IAOPContext context)
        {
            _container = context.ServiceProvider.GetService<TransactionContainer>();

            try
            {
                if (_container.TransactionOpened)
                {
                    _container.CommitTransactionAsync();
                    _container.DisposeTransaction();

                }
            }
            catch (Exception ex)
            {
                _container.RollbackTransactionAsync();
                throw new Exception("系统异常", ex);
            }

            if (_container.TransactionOpened)
            {
                _container.TransactionOpened = false;
            }

        }
    }

    [Component(Lifetime = Wombat.Core.DependencyInjection.ServiceLifetime.Singleton)]
    public class TransactionContainer 
    {
        public TransactionContainer(IServiceProvider serviceProvider)
        {
            //_distributedTransaction = FreeSqlCloud();

            //var allRepositoryInterfaces = GlobalAssemblies.AllTypes.Where(x =>
            //        typeof(IFreeSql).IsAssignableFrom(x)
            //        && x.IsInterface
            //        && x != typeof(IFreeSql)
            //    ).ToList();
            //allRepositoryInterfaces.Add(typeof(IFreeSql));

            //var repositories = allRepositoryInterfaces
            //    .Select(x => serviceProvider.GetService(x) as IFreeSql)
            //    .ToArray();

            //_distributedTransaction.AddDbAccessor(repositories);

           var  _freesql = serviceProvider.GetService<IFreeSql>();
            _dbConnection = _freesql.Ado.MasterPool.Get();


        }

        private Object<DbConnection> _dbConnection;

        private DbTransaction _transaction;


        public bool TransactionOpened { get; set; }

        public void Dispose()
        {
            _dbConnection.Dispose();
        }

        //public Task<(bool Success, Exception ex)> RunTransactionAsync(Func<Task> action, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        //{
        //    //using (var uow = _freesql.CreateUnitOfWork())
        //    //{
        //    //    try
        //    //    {
        //    //        //指定事务级别
        //    //        uow.IsolationLevel = isolationLevel;
        //    //        //开启事务
        //    //        var tran = uow.GetOrBeginTransaction();
        //    //        //执行逻辑
        //    //        action();

        //    //        //提交事务
        //    //        uow.Commit();

        //    //        return new Task<(bool Success, Exception ex)>(() => { return (true, null); });
        //    //    }
        //    //    catch (Exception ex)
        //    //    {
        //    //        //回滚事务
        //    //        uow.Rollback();
        //    //        //抛出异常
        //    //        throw;
        //    //    }

        //    //}
        //}

        //public (bool Success, Exception ex) RunTransaction(Action action, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        //{
        //    using (var uow = _freesql.CreateUnitOfWork())
        //    {
        //        try
        //        {
        //            //指定事务级别
        //            uow.IsolationLevel = isolationLevel;
        //            //开启事务
        //            var tran = uow.GetOrBeginTransaction();
        //            //执行逻辑
        //            action();

        //            //提交事务
        //            uow.Commit();

        //            return (true, null);
        //        }
        //        catch (Exception ex)
        //        {
        //            //回滚事务
        //            uow.Rollback();
        //            //抛出异常
        //            throw;
        //        }

        //    }
        //}

        public async Task BeginTransactionAsync(IsolationLevel isolationLevel)
        {
            await Task.Run(()=>{ _transaction = _dbConnection.Value.BeginTransactionAsync().Result; }); 
        }

        public async Task CommitTransactionAsync()
        {
           await _transaction.CommitAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            await _transaction.RollbackAsync();
        }

        public async Task DisposeTransaction()
        {
            TransactionOpened = false;
            await _transaction.DisposeAsync();
        }

        public void AddDbAccessor(params IFreeSql[] repositories)
        {
            //_distributedTransaction.AddDbAccessor(repositories);
        }
    }
}
