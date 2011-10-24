package com.hazelcast.security.impl;

import java.security.Permission;
import java.util.concurrent.ExecutorService;

import static com.hazelcast.security.SecurityUtil.*;

import com.hazelcast.core.Prefix;
import com.hazelcast.impl.*;
import com.hazelcast.impl.FactoryImpl.ProxyKey;
import com.hazelcast.security.SecurityConstants;
import com.hazelcast.security.SecurityUtil;
import com.hazelcast.security.permission.*;

public class SecureProxyFactory extends DefaultProxyFactory implements ProxyFactory {
	
	private final Node node;
	
	public SecureProxyFactory(Node node) {
		super(node.factory);
		this.node = node;
	}

	@Override
	public MProxy createMapProxy(String name) {
		if(name.startsWith(Prefix.MAP_FOR_QUEUE)
				|| name.startsWith(Prefix.MAP_HAZELCAST)
				|| name.startsWith(Prefix.MAP_OF_LIST)) {
			return super.createMapProxy(name); 
		}
		return new SecureMProxy(node, super.createMapProxy(name));
	}

	@Override
	public QProxy createQueueProxy(String name) {
		return new SecureQProxy(node, super.createQueueProxy(name));
	}

	@Override
	public TopicProxy createTopicProxy(String name) {
		return new SecureTopicProxy(node, super.createTopicProxy(name));
	}

	@Override
	public MultiMapProxy createMultiMapProxy(String name) {
		return new SecureMultimapProxy(node, super.createMultiMapProxy(name));
	}

	@Override
	public ListProxy createListProxy(String name) {
		return new SecureListProxy(node, super.createListProxy(name));
	}

	@Override
	public SetProxy createSetProxy(String name) {
		return new SecureSetProxy(node, super.createSetProxy(name));
	}

	@Override
	public LockProxy createLockProxy(Object key) {
		return new SecureLockProxy(node, super.createLockProxy(key));
	}

	@Override
	public AtomicNumberProxy createAtomicNumberProxy(String name) {
		return new SecureAtomicNumberProxy(node, super.createAtomicNumberProxy(name));
	}

	@Override
	public SemaphoreProxy createSemaphoreProxy(String name) {
		return new SecureSemaphoreProxy(node, super.createSemaphoreProxy(name));
	}

	@Override
	public CountDownLatchProxy createCountDownLatchProxy(String name) {
		return new SecureCountDownLatchProxy(node, super.createCountDownLatchProxy(name));
	}

	@Override
	public IdGeneratorProxy createIdGeneratorProxy(String name) {
		return super.createIdGeneratorProxy(name);
	}

	@Override
	public ExecutorService createExecutorServiceProxy(String name) {
		SecurityUtil.checkPermission(node.securityContext, 
				new ExecutorServicePermission(getShortInstanceName(name), SecurityConstants.ACTION_CREATE));
		return new SecureExecutorServiceProxy(node, super.createExecutorServiceProxy(name), 
				getShortInstanceName(name));
	}

	@Override
	public TransactionImpl createTransaction() {
		SecurityUtil.checkPermission(node.securityContext, new TransactionPermission());
		return super.createTransaction();
	}

	public void checkProxyPermission(ProxyKey proxyKey) {
		final String name = proxyKey.getName();
		final String action = SecurityConstants.ACTION_CREATE;
		final String actualName = getShortInstanceName(name);
		Permission p = null;
		
		if (name.startsWith(Prefix.MAP)) {
			p = new MapPermission(actualName, action);
		} else if (name.startsWith(Prefix.QUEUE)) {
			p = new QueuePermission(actualName, action);
		} else if (name.startsWith(Prefix.TOPIC)) {
			p = new TopicPermission(actualName, action);
		} else if (name.startsWith(Prefix.AS_LIST)) {
			p = new ListPermission(actualName, action);
		} else if (name.startsWith(Prefix.MULTIMAP)) {
			p = new MultiMapPermission(actualName, action);
		} else if (name.startsWith(Prefix.SET)) {
			p = new SetPermission(actualName, action);
		} else if (name.startsWith(Prefix.ATOMIC_NUMBER)) {
			p = new AtomicNumberPermission(actualName, action);
		} else if (name.startsWith(Prefix.IDGEN)) {
		} else if (name.startsWith(Prefix.SEMAPHORE)) {
			p = new SemaphorePermission(actualName, action);
		} else if (name.startsWith(Prefix.COUNT_DOWN_LATCH)) {
			p = new CountDownLatchPermission(actualName, action);
		} else if (name.equals("lock")) {
			p = new LockPermission(proxyKey.getKey(), action);
		}
		SecurityUtil.checkPermission(node.securityContext, p);
	}
}
